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
        private readonly Address DomesticAddress1;
        private readonly Address DomesticAddress2;
        private readonly Address InternationalAddress1;
        private readonly Address InternationalAddress2;
        private readonly Package Package1;
        private readonly Package Package2;
        private readonly string USPSUserId;

        public USPSInternationalRates()
        {
            DomesticAddress1 = new Address("278 Buckley Jones Road", "", "", "Cleveland", "MS", "38732", "US");
            DomesticAddress2 = new Address("One Microsoft Way", "", "", "Redmond", "WA", "98052", "US");
            InternationalAddress1 = new Address("Jubail", "Jubail", "31951", "Saudi Arabia"); //has limited intl services available
            InternationalAddress2 = new Address("80-100 Victoria St", "", "", "London SW1E 5JL", "", "", "United Kingdom");

            Package1 = new Package(4, 4, 4, 5, 0);
            Package2 = new Package(6, 6, 6, 5, 100);

            USPSUserId = ConfigurationManager.AppSettings["USPSUserId"];
        }

        [Fact]
        public void USPS_Intl_Returns_Multiple_Rates_When_Using_Multiple_Packages_For_All_Services_And_Multiple_Packages()
        {
            var packages = new List<Package>();
            packages.Add(Package1);
            packages.Add(Package2);

            var rateManager = new RateManager();
            rateManager.AddProvider(new USPSInternationalProvider(USPSUserId));

            var response = rateManager.GetRates(DomesticAddress1, InternationalAddress2, packages);

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
            rateManager.AddProvider(new USPSInternationalProvider(USPSUserId));

            var response = rateManager.GetRates(DomesticAddress1, InternationalAddress2, Package1);

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
            rateManager.AddProvider(new USPSInternationalProvider(USPSUserId));

            var response = rateManager.GetRates(DomesticAddress1, DomesticAddress2, Package1);

            Debug.WriteLine(string.Format("Rates returned: {0}", response.Rates.Any() ? response.Rates.Count.ToString() : "0"));

            Assert.NotNull(response);
            Assert.Empty(response.Rates);
        }

        [Fact]
        public void USPS_Intl_Returns_No_Rates_When_Using_Invalid_Addresses_For_Single_Service()
        {
            //can't rate intl with a domestic address

            var rateManager = new RateManager();
            rateManager.AddProvider(new USPSInternationalProvider(USPSUserId, "Priority Mail International"));

            var response = rateManager.GetRates(DomesticAddress1, DomesticAddress2, Package1);

            Debug.WriteLine(string.Format("Rates returned: {0}", response.Rates.Any() ? response.Rates.Count.ToString() : "0"));

            Assert.NotNull(response);
            Assert.Empty(response.Rates);
        }

        [Fact]
        public void USPS_Intl_Returns_Single_Rate_When_Using_Valid_Addresses_For_Single_Service()
        {
            var rateManager = new RateManager();
            rateManager.AddProvider(new USPSInternationalProvider(USPSUserId, "Priority Mail International"));

            var response = rateManager.GetRates(DomesticAddress1, InternationalAddress1, Package1);

            Debug.WriteLine(string.Format("Rates returned: {0}", response.Rates.Any() ? response.Rates.Count.ToString() : "0"));

            Assert.NotNull(response);
            Assert.NotEmpty(response.Rates);
            Assert.Empty(response.ServerErrors);
            Assert.Equal(response.Rates.Count, 1);
            Assert.True(response.Rates.First().TotalCharges > 0);

            Debug.WriteLine(response.Rates.First().Name + ": " + response.Rates.First().TotalCharges);
        }
    }
}