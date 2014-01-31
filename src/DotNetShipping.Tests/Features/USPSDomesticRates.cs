using System.Configuration;
using System.Diagnostics;
using System.Linq;

using DotNetShipping.ShippingProviders;

using Xunit;

namespace DotNetShipping.Tests.Features
{
    public class USPSDomesticRates
    {
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
        private Address DomesticAddress1;
        private Address DomesticAddress2;
        private Address InternationalAddress1;

        private Package RealPackage;

        private string USPSUserId;

        #endregion

        #region ctor
        public USPSDomesticRates()
        {
            DomesticAddress1 = new Address("278 Buckley Jones Road", "", "", "Cleveland", "MS", "38732", "US");
            DomesticAddress2 = new Address("One Microsoft Way", "", "", "Redmond", "WA", "98052", "US");
            InternationalAddress1 = new Address("Jubail", "Jubail", "31951", "Saudi Arabia");

            RealPackage = new Package(4, 4, 4, 5, 0);

            USPSUserId = ConfigurationManager.AppSettings["USPSUserId"];
        }
        #endregion

        #region test methods

        [Fact]
        public void USPS_Domestic_Returns_No_Rates_When_Using_Invalid_Addresses_For_All_Services()
        {
            string uspsUserId = USPSUserId;
            var package = RealPackage;
            var origin = DomesticAddress1;
            var destination = InternationalAddress1; //can't rate domestic with an intl address
            var rateManager = new RateManager();
            rateManager.AddProvider(new USPSProvider(uspsUserId));

            Shipment response = rateManager.GetRates(origin, destination, package);

            Debug.WriteLine(string.Format("Rates returned: {0}", response.Rates.Any() ? response.Rates.Count.ToString() : "0"));

            Assert.NotNull(response);
            Assert.Empty(response.Rates);
            Assert.Empty(response.ServerErrors);
        }

        [Fact]
        public void USPS_Domestic_Returns_No_Rates_When_Using_Invalid_Addresses_For_Single_Service()
        {
            string uspsUserId = USPSUserId;
            var package = RealPackage;
            var origin = DomesticAddress1; //can't rate intl with a domestic address
            var destination = InternationalAddress1;
            var rateManager = new RateManager();
            rateManager.AddProvider(new USPSProvider(uspsUserId, "Priority Mail"));

            Shipment response = rateManager.GetRates(origin, destination, package);

            Debug.WriteLine(string.Format("Rates returned: {0}", response.Rates.Any() ? response.Rates.Count.ToString() : "0"));

            Assert.NotNull(response);
            Assert.Empty(response.Rates);
            Assert.Empty(response.ServerErrors);
        }

        [Fact]
        public void USPS_Domestic_Returns_Multiple_Rates_When_Using_Valid_Addresses_For_All_Services()
        {
            string uspsUserId = USPSUserId;
            var package = RealPackage;
            var origin = DomesticAddress1;
            var destination = DomesticAddress2;
            var rateManager = new RateManager();
            rateManager.AddProvider(new USPSProvider(uspsUserId));

            Shipment response = rateManager.GetRates(origin, destination, package);

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
            string uspsUserId = USPSUserId;
            var package = RealPackage;
            var origin = DomesticAddress1;
            var destination = DomesticAddress2;
            var rateManager = new RateManager();
            rateManager.AddProvider(new USPSProvider(uspsUserId, "Priority Mail"));

            Shipment response = rateManager.GetRates(origin, destination, package);

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