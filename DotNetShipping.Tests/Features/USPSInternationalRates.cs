using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;

using DotNetShipping.ShippingProviders;

using Xunit;

namespace DotNetShipping.Tests.Features
{
    public class USPSInternationalRates
    {
        private readonly Address _domesticAddress1;
        private readonly Address _domesticAddress2;
        private readonly Address _internationalAddress1;
        private readonly Address _internationalAddress2;
        private readonly Package _package1;
        private readonly Package _package2;
        private readonly string _uspsUserId;

        public USPSInternationalRates()
        {
            _domesticAddress1 = new Address("278 Buckley Jones Road", "", "", "Cleveland", "MS", "38732", "US");
            _domesticAddress2 = new Address("One Microsoft Way", "", "", "Redmond", "WA", "98052", "US");
            _internationalAddress1 = new Address("Jubail", "Jubail", "31951", "SA"); //has limited intl services available
            _internationalAddress2 = new Address("80-100 Victoria St", "", "", "London", "", "SW1E 5JL", "GB");

            _package1 = new Package(14, 14, 14, 15, 0);
            _package2 = new Package(6, 6, 6, 5, 100);

            _uspsUserId = ConfigurationManager.AppSettings["USPSUserId"];
        }

        [Fact]
        public void USPS_Intl_Returns_Multiple_Rates_When_Using_Multiple_Packages_For_All_Services_And_Multiple_Packages()
        {
            var packages = new List<Package>();
            packages.Add(_package1);
            packages.Add(_package2);

            var rateManager = new RateManager();
            rateManager.AddProvider(new USPSInternationalProvider(_uspsUserId));

            var response = rateManager.GetRates(_domesticAddress1, _internationalAddress2, packages);

            Debug.WriteLine(string.Format("Rates returned: {0}", response.Rates.Any() ? response.Rates.Count.ToString() : "0"));

            Assert.NotNull(response);
            Assert.NotEmpty(response.Rates);
            Assert.Empty(response.ServerErrors);

            foreach (var rate in response.Rates)
            {
                Assert.NotNull(rate);
                Assert.True(rate.TotalCharges > 0);

                Debug.WriteLine(rate.Name + ": " + rate.TotalCharges);
            }
        }

        [Fact]
        public void USPS_Intl_Returns_Multiple_Rates_When_Using_Valid_Addresses_For_All_Services()
        {
            var rateManager = new RateManager();
            rateManager.AddProvider(new USPSInternationalProvider(_uspsUserId));

            var response = rateManager.GetRates(_domesticAddress1, _internationalAddress2, _package1);

            Debug.WriteLine(string.Format("Rates returned: {0}", response.Rates.Any() ? response.Rates.Count.ToString() : "0"));

            Assert.NotNull(response);
            Assert.NotEmpty(response.Rates);
            Assert.Empty(response.ServerErrors);

            foreach (var rate in response.Rates)
            {
                Assert.NotNull(rate);
                Assert.True(rate.TotalCharges > 0);

                Debug.WriteLine(rate.Name + ": " + rate.TotalCharges);
            }
        }

        [Fact]
        public void USPS_Intl_Returns_No_Rates_When_Using_Invalid_Addresses_For_All_Services()
        {
            var rateManager = new RateManager();
            rateManager.AddProvider(new USPSInternationalProvider(_uspsUserId));

            var response = rateManager.GetRates(_domesticAddress1, _domesticAddress2, _package1);

            Debug.WriteLine(string.Format("Rates returned: {0}", response.Rates.Any() ? response.Rates.Count.ToString() : "0"));

            Assert.NotNull(response);
            Assert.Empty(response.Rates);
        }

        [Fact]
        public void USPS_Intl_Returns_No_Rates_When_Using_Invalid_Addresses_For_Single_Service()
        {
            //can't rate intl with a domestic address

            var rateManager = new RateManager();
            rateManager.AddProvider(new USPSInternationalProvider(_uspsUserId, "Priority Mail International"));

            var response = rateManager.GetRates(_domesticAddress1, _domesticAddress2, _package1);

            Debug.WriteLine(string.Format("Rates returned: {0}", response.Rates.Any() ? response.Rates.Count.ToString() : "0"));

            Assert.NotNull(response);
            Assert.Empty(response.Rates);
        }

        [Fact]
        public void USPS_Intl_Returns_Single_Rate_When_Using_Valid_Addresses_For_Single_Service()
        {
            var rateManager = new RateManager();
            rateManager.AddProvider(new USPSInternationalProvider(_uspsUserId, "Priority Mail International"));

            var response = rateManager.GetRates(_domesticAddress1, _internationalAddress1, _package1);

            Debug.WriteLine(string.Format("Rates returned: {0}", response.Rates.Any() ? response.Rates.Count.ToString() : "0"));

            Assert.NotNull(response);
            Assert.NotEmpty(response.Rates);
            Assert.Empty(response.ServerErrors);
            Assert.Equal(response.Rates.Count, 1);
            Assert.True(response.Rates.First().TotalCharges > 0);

            Debug.WriteLine(response.Rates.First().Name + ": " + response.Rates.First().TotalCharges);
        }

        [Fact]
        public void CanGetUspsInternationalServiceCodes()
        {
            var provider = new USPSInternationalProvider();
            var serviceCodes = provider.GetServiceCodes();

            Assert.NotNull(serviceCodes);
            Assert.NotEmpty(serviceCodes);
        }
    }
}
