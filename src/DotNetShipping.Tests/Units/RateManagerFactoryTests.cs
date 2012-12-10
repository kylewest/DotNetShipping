using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DotNetShipping.ShippingProviders;

using Xunit;

namespace DotNetShipping.Tests.Units
{
    public class RateManagerFactoryTests
    {
        [Fact]
        public void InstantiatesRateManagerUsingReflection()
        {
            var rateManager = RateManagerFactory.Build();

            Assert.NotNull(rateManager);

            //Todo: Needs better assertion
        }
    }
}
