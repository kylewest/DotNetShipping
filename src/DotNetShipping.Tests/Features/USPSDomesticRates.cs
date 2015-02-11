using System.Configuration;
using System.Diagnostics;
using System.Linq;

using DotNetShipping.ShippingProviders;

using Xunit;

namespace DotNetShipping.Tests.Features
{
    public class USPSDomesticRates
    {
        #region ctor

        public USPSDomesticRates()
        {
            DomesticAddress1 = new Address("278 Buckley Jones Road", "", "", "Cleveland", "MS", "38732", "US");
            DomesticAddress2 = new Address("One Microsoft Way", "", "", "Redmond", "WA", "98052", "US");
            InternationalAddress1 = new Address("Jubail", "Jubail", "31951", "Saudi Arabia");

            Package1 = new Package(4, 4, 4, 5, 0);
            Package1 = new Package(6, 6, 6, 5, 100);

            USPSUserId = ConfigurationManager.AppSettings["USPSUserId"];
        }

        #endregion

        /*
         * These tests are for basic functionality only. There are several restrictions
         * for USPS Domestic mail that limit sizes, weights, and services allowed
         * by country. A full list of restrictions can be cound on the USPS web site
         * at: https://www.usps.com/ship/compare-domestic-services.htm
         *
         * Also, USPS did away with the Priority Mail "bucket" name and break it into
         * Priority Mail 1-Day, Priority Mail 2-Day, and Priority Mail 3-Day. The rates
         * are supposed to be the same but the names are different and are chosen based
         * on the zip codes provided. Express Mail was changed to Priority Mail Express
         * and has the same N-Day names as regular Priority Mail. Info on it is at:
         * https://www.usps.com/priority-mail/
         */

        #region properties

        private readonly Address DomesticAddress1;
        private readonly Address DomesticAddress2;
        private readonly Address InternationalAddress1;

        private readonly Package Package1;
        private Package Package2;

        private readonly string USPSUserId;

        #endregion

        #region test methods

        [Fact]
        public void USPS_Domestic_Returns_No_Rates_When_Using_Invalid_Addresses_For_All_Services()
        {
            var rateManager = new RateManager();
            rateManager.AddProvider(new USPSProvider(USPSUserId));

            Shipment response = rateManager.GetRates(DomesticAddress1, InternationalAddress1, Package1);

            Debug.WriteLine(string.Format("Rates returned: {0}", response.Rates.Any() ? response.Rates.Count.ToString() : "0"));

            Assert.NotNull(response);
            Assert.Empty(response.Rates);
            Assert.Empty(response.ServerErrors);
        }

        [Fact]
        public void USPS_Domestic_Returns_No_Rates_When_Using_Invalid_Addresses_For_Single_Service()
        {
            var rateManager = new RateManager();
            rateManager.AddProvider(new USPSProvider(USPSUserId, "Priority Mail"));

            Shipment response = rateManager.GetRates(DomesticAddress1, InternationalAddress1, Package1);

            Debug.WriteLine(string.Format("Rates returned: {0}", response.Rates.Any() ? response.Rates.Count.ToString() : "0"));

            Assert.NotNull(response);
            Assert.Empty(response.Rates);
            Assert.Empty(response.ServerErrors);
        }

        [Fact]
        public void USPS_Domestic_Returns_Multiple_Rates_When_Using_Valid_Addresses_For_All_Services()
        {
            var rateManager = new RateManager();
            rateManager.AddProvider(new USPSProvider(USPSUserId));

            Shipment response = rateManager.GetRates(DomesticAddress1, DomesticAddress2, Package1);

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
        public void USPS_Domestic_Returns_Multiple_Rates_When_Using_Valid_Addresses_For_All_Services_And_Multiple_Packages()
        {
            var rateManager = new RateManager();
            rateManager.AddProvider(new USPSProvider(USPSUserId));

            Shipment response = rateManager.GetRates(DomesticAddress1, DomesticAddress2, Package1);

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
        public void USPS_Domestic_Returns_Single_Rate_When_Using_Valid_Addresses_For_Single_Service()
        {
            var rateManager = new RateManager();
            rateManager.AddProvider(new USPSProvider(USPSUserId, "Priority Mail"));

            Shipment response = rateManager.GetRates(DomesticAddress1, DomesticAddress2, Package1);

            Debug.WriteLine(string.Format("Rates returned: {0}", response.Rates.Any() ? response.Rates.Count.ToString() : "0"));

            Assert.NotNull(response);
            Assert.NotEmpty(response.Rates);
            Assert.Empty(response.ServerErrors);
            Assert.Equal(response.Rates.Count, 1);
            Assert.True(response.Rates.First().TotalCharges > 0);

            Debug.WriteLine(response.Rates.First().Name + ": " + response.Rates.First().TotalCharges);
        }

        #endregion
    }
}
