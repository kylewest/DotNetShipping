using Xunit;

namespace DotNetShipping.Tests.Units
{
    public class PackageTests
    {
        [Theory]
        [InlineData(3.5, 3, 8)]
        [InlineData(5.8, 5, 13)]
        [InlineData(6.2, 6, 4)]
        public void PoundsAndOuncesCalculatedCorrectly(decimal weight, int pounds, int ounces)
        {
            var package = new Package(1, 2, 3, weight, 100);
            Assert.Equal(package.PoundsAndOunces.Pounds, pounds);
            Assert.Equal(package.PoundsAndOunces.Ounces, ounces);
        }
    }
}