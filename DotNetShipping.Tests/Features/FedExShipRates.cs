using System;
using System.Configuration;
using System.Linq;

using DotNetShipping.ShippingProviders;

using Xunit;

namespace DotNetShipping.Tests.Features
{
    public abstract class FedExShipRatesTestsBase : IDisposable
    {
        protected readonly RateManager _rateManager;

        protected FedExShipRatesTestsBase()
        {
            var appSettings = ConfigurationManager.AppSettings;
            var fedexKey = appSettings["FedExKey"];
            var fedexPassword = appSettings["FedExPassword"];
            var fedexAccountNumber = appSettings["FedExAccountNumber"];
            var fedexMeterNumber = appSettings["FedExMeterNumber"];
            var fedexUseProduction = Convert.ToBoolean(appSettings["FedExUseProduction"]);

            _rateManager = new RateManager();
            _rateManager.AddProvider(new FedExProvider(fedexKey, fedexPassword, fedexAccountNumber, fedexMeterNumber, fedexUseProduction));
        }

        public void Dispose()
        {
            
        }
    }

    public class FedExShipRates : FedExShipRatesTestsBase
    {
        [Fact]
        public void FedExReturnsRates()
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
            }
        }

        [Fact]
        public void FedExReturnsDifferentRatesForSignatureOnDelivery()
        {
            var from = new Address("Annapolis", "MD", "21401", "US");
            var to = new Address("Fitchburg", "WI", "53711", "US");

            var nonSignaturePackage = new Package(7, 7, 7, 6, 0, null, false);
            var signaturePackage = new Package(7, 7, 7, 6, 0, null, true);

            // Non signature rates first
            var nonSignatureRates = _rateManager.GetRates(from, to, nonSignaturePackage);
            var fedExNonSignatureRates = nonSignatureRates.Rates.ToList();

            Assert.NotNull(nonSignatureRates);
            Assert.True(fedExNonSignatureRates.Any());

            foreach (var rate in fedExNonSignatureRates)
            {
                Assert.True(rate.TotalCharges > 0);
            }
            
            var signatureRates = _rateManager.GetRates(from, to, signaturePackage);
            var fedExSignatureRates = signatureRates.Rates.ToList();

            Assert.NotNull(signatureRates);
            Assert.True(fedExSignatureRates.Any());

            foreach (var rate in fedExSignatureRates)
            {
                Assert.True(rate.TotalCharges > 0);
            }

            // Now compare prices
            foreach (var signatureRate in fedExSignatureRates)
            {
                var nonSignatureRate = fedExNonSignatureRates.FirstOrDefault(x => x.Name == signatureRate.Name);

                if (nonSignatureRate != null)
                {
                    var signatureTotalCharges = signatureRate.TotalCharges;
                    var nonSignatureTotalCharges = nonSignatureRate.TotalCharges;
                    Assert.NotEqual(signatureTotalCharges, nonSignatureTotalCharges);
                }
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