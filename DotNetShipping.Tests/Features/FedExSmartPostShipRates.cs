using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DotNetShipping.ShippingProviders;

using Xunit;

namespace DotNetShipping.Tests.Features
{
    public abstract class FedExSmartPostShipRatesTestsBase : IDisposable
    {
        protected readonly RateManager _rateManager;

        protected FedExSmartPostShipRatesTestsBase()
        {
            var appSettings = ConfigurationManager.AppSettings;
            var fedexKey = appSettings["FedExKey"];
            var fedexPassword = appSettings["FedExPassword"];
            var fedexAccountNumber = appSettings["FedExAccountNumber"];
            var fedexMeterNumber = appSettings["FedExMeterNumber"];
            var fedexHubId = appSettings["FedExHubId"];
            var fedexUseProduction = Convert.ToBoolean(appSettings["FedExUseProduction"]);

            _rateManager = new RateManager();
            _rateManager.AddProvider(new FedExSmartPostProvider(fedexKey, fedexPassword, fedexAccountNumber, fedexMeterNumber, fedexHubId, fedexUseProduction));
        }

        public void Dispose()
        {

        }
    }

    public class FedExSmartPostShipRates : FedExSmartPostShipRatesTestsBase
    {
        [Fact]
        public void FedExSmartPostReturnsRates()
        {
            var from = new Address("Annapolis", "MD", "21401", "US");
            var to = new Address("Fitchburg", "WI", "53711", "US");
            var package = new Package(7, 7, 7, 6, 0);

            var r = _rateManager.GetRates(from, to, package);
            var fedExRates = r.Rates.ToList();

            Assert.NotNull(r);
            Assert.True(fedExRates.Any());

            foreach (var rate in fedExRates)
            {
                Assert.True(rate.TotalCharges > 0);
                Assert.Equal(rate.ProviderCode, "SMART_POST");
            }
        }

        [Fact]
        public void CanGetFedExServiceCodes()
        {
            var provider = new FedExProvider();
            var serviceCodes = provider.GetServiceCodes();

            Assert.NotNull(serviceCodes);
            Assert.NotEmpty(serviceCodes);
        }
    }
}
