using System.Configuration;
using System.Linq;

using DotNetShipping.ShippingProviders;

using Xunit;

namespace DotNetShipping.Tests.Features
{
    public class FedExShipRates
    {
        /*  FedEx Rate Lookup
         *  https://www.fedex.com/ratefinder/home?cc=US&language=en&locId=express
         */

        #region Methods

        
        public void FedExReturnsRates()
        {
            RateManager rateManager = RateManagerFactory.Build();

            var from = new Address("Salt Lake City", "UT", "84119", "US");
            var to = new Address("Beverly Hills", "CA", "90210", "US");
            var package = new Package(12, 12, 12, 12, 0);

            Shipment r = rateManager.GetRates(from, to, package);

            var fedExRates = r.Rates.Where(x => x.Provider == "FedEx").ToList();

            Assert.NotNull(r);
            Assert.True(fedExRates.Any());

            foreach (var rate in fedExRates)
            {
                Assert.True(rate.TotalCharges > 0);
            }
        }

        #endregion
    }

    
    public class USPSInternationalRates
    {
        [Fact]
        public void USPSInternationalReturnsRate()
        {
            string uspsUserId = ConfigurationManager.AppSettings["USPSUserId"];
            
            var package = new Package(4, 4, 4, 4, 0);

            var origin = new Address("278 Buckley Jones Road", "", "", "Cleveland", "MS", "38732", "US");
            var destination = new Address("Jubail", "Jubail", "31951", "Saudi Arabia");
            var rateManager = new RateManager();
            rateManager.AddProvider(new USPSInternationalProvider(uspsUserId, "Priority Mail International"));

            Shipment response = rateManager.GetRates(origin, destination, package);

            Assert.NotNull(response);
            
        }
    }
}