using System.Linq;

using Xunit;

namespace DotNetShipping.Tests.Features
{
    public class FedExShipRates
    {
        /*  FedEx Rate Lookup
         *  https://www.fedex.com/ratefinder/home?cc=US&language=en&locId=express
         */

        #region Methods

        [Fact]
        public void FedExReturnsRates()
        {
            RateManager rateManager = RateManagerFactory.Build();

            var from = new Address("Salt Lake City", "UT", "84119", "US");
            var to = new Address("Beverly Hills", "CA", "90210", "US");
            var package = new Package(12, 12, 12, 12, 0);

            Shipment r = rateManager.GetRates(from, to, package);

            Assert.NotNull(r);
            Assert.True(r.rates.Any());

            foreach (var rate in r.rates)
            {
                Assert.True(rate.TotalCharges > 0);
            }
        }

        #endregion
    }
}