using System.Linq;

using Xunit;

namespace DotNetShipping.Tests.Features
{
    public class FedExShipRates
    {
        [Fact]
        public void FedExReturnsRates()
        {
            var rateManager = RateManagerFactory.Build();

            var from = new Address("Salt Lake City", "UT", "84119", "US");
            var to = new Address("Beverly Hills", "CA", "90210", "US");
            var package = new Package(12, 12, 12, 12, 0);

            var r = rateManager.GetRates(from, to, package);

            var fedExRates = r.Rates.Where(x => x.Provider == "FedEx").ToList();

            Assert.NotNull(r);
            Assert.True(fedExRates.Any());

            foreach (var rate in fedExRates)
            {
                Assert.True(rate.TotalCharges > 0);
            }
        }
    }
}