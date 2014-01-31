using System.Configuration;
using System.Diagnostics;
using System.Linq;

using DotNetShipping.ShippingProviders;

using Xunit;

namespace DotNetShipping.Tests.Features
{
    public class UPSRates
    {
        #region Notes - READ ME!
        /*
        * Valid domestic values:
        * 14 = Next Day Air Early AM,
        * 01 = Next Day Air,
        * 13 = Next Day Air Saver,
        * 59 = 2nd Day Air AM,
        * 02 = 2nd Day Air,
        * 12 = 3 Day Select,
        * 03 = Ground
        * 
        * Specialty Codes:
        * 93 = UPS Sure Post. Customer must register for this service before the API will return a response for it.
        * 
        * Valid international values:
        * 11 = Standard,  //Canada, US, & Mexico only
        * 07 = Worldwide Express,
        * 54 = Worldwide Express Plus,
        * 08 = Worldwide Expedited,
        * 65 = Saver. Required for Rating and Ignored for Shopping.
        * 
        * Valid Poland to Poland Same Day values:
        * 82 = UPS Today Standard,
        * 83 = UPS Today Dedicated Courier,
        * 84 = UPS Today Intercity,
        * 85 = UPS Today Express,
        * 86 = UPS Today Express Saver
        * 96 = UPS World Wide Express Freight
        */
        #endregion

        #region properties
        private Address DomesticAddress1;
        private Address DomesticAddress2;
        private Address InternationalAddress1;
        private Address InternationalAddress2;

        private Package RealPackage;

        private string UPSUserId;
        private string UPSPassword;
        private string UPSLicenseNumber;

        #endregion

        #region ctor
        public UPSRates()
        {
            DomesticAddress1 = new Address("278 Buckley Jones Road", "", "", "Cleveland", "MS", "38732", "US");
            DomesticAddress2 = new Address("One Microsoft Way", "", "", "Redmond", "WA", "98052", "US");
            InternationalAddress1 = new Address("Porscheplatz 1", "", "", "70435 Stuttgart", "", "", "DE");
            InternationalAddress2 = new Address("80-100 Victoria St", "", "", "London SW1E 5JL", "", "", "GB");

            RealPackage = new Package(4, 4, 4, 5, 0);

            UPSUserId = ConfigurationManager.AppSettings["UPSUserId"];
            UPSPassword = ConfigurationManager.AppSettings["UPSPassword"];
            UPSLicenseNumber = ConfigurationManager.AppSettings["UPSLicenseNumber"];
        }
        #endregion

        #region test methods

        [Fact]
        public void UPS_Returns_Rates_When_Using_International_Destination_Addresses_For_All_Services()
        {
            var package = RealPackage;
            var origin = DomesticAddress1;
            var destination = InternationalAddress1; //can't rate domestic with an intl address
            var rateManager = new RateManager();
            rateManager.AddProvider(new UPSProvider(UPSLicenseNumber, UPSUserId, UPSPassword));

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
        public void UPS_Domestic_Returns_Rates_When_Using_International_Addresses_For_Single_Service()
        {
            var package = RealPackage;
            var origin = DomesticAddress1; //can't rate intl with a domestic address
            var destination = InternationalAddress1;
            var rateManager = new RateManager();
            rateManager.AddProvider(new UPSProvider(UPSLicenseNumber, UPSUserId, UPSPassword, "UPS Worldwide Express"));

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
        public void UPS_Returns_Multiple_Rates_When_Using_Valid_Addresses_For_All_Services()
        {
            var package = RealPackage;
            var origin = DomesticAddress1;
            var destination = DomesticAddress2;
            var rateManager = new RateManager();
            rateManager.AddProvider(new UPSProvider(UPSLicenseNumber, UPSUserId, UPSPassword));

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
        public void UPS_Returns_Single_Rate_When_Using_Domestic_Addresses_For_Single_Service()
        {
            var package = RealPackage;
            var origin = DomesticAddress1;
            var destination = DomesticAddress2;
            var rateManager = new RateManager();
            rateManager.AddProvider(new UPSProvider(UPSLicenseNumber, UPSUserId, UPSPassword, "UPS Ground"));

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