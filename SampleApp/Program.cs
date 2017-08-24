using System;
using System.Collections.Generic;
using System.Configuration;

using DotNetShipping.ShippingProviders;

namespace DotNetShipping.SampleApp
{
    internal class Program
    {
        private static void Main()
        {
            var appSettings = ConfigurationManager.AppSettings;

            // You will need a license #, userid and password to utilize the UPS provider.
            var upsLicenseNumber = appSettings["UPSLicenseNumber"];
            var upsUserId = appSettings["UPSUserId"];
            var upsPassword = appSettings["UPSPassword"];

            // You will need an account # and meter # to utilize the FedEx provider.
            var fedexKey = appSettings["FedExKey"];
            var fedexPassword = appSettings["FedExPassword"];
            var fedexAccountNumber = appSettings["FedExAccountNumber"];
            var fedexMeterNumber = appSettings["FedExMeterNumber"];
            var fedexHubId = appSettings["FedExHubId"]; // 5531 is the hubId to use in FedEx's test environment
            var fedexUseProduction = Convert.ToBoolean(appSettings["FedExUseProduction"]);

            // You will need a userId to use the USPS provider. Your account will also need access to the production servers.
            var uspsUserId = appSettings["USPSUserId"];

            // Setup package and destination/origin addresses
            var packages = new List<Package>();
            packages.Add(new Package(12, 12, 12, 35, 150));
            packages.Add(new Package(4, 4, 6, 15, 250));

            var origin = new Address("", "", "06405", "US");
            var destination = new Address("", "", "20852", "US"); // US Address
            //var destination = new Address("", "", "00907", "PR"); // Puerto Rico Address
            //var destination = new Address("", "", "L4W 1S2", "CA"); // Canada Address
            //var destination = new Address("", "", "SW1E 5JL", "GB"); // UK Address

            // Create RateManager
            var rateManager = new RateManager();

            // Add desired DotNetShippingProviders
            rateManager.AddProvider(new UPSProvider(upsLicenseNumber, upsUserId, upsPassword) {UseProduction = false});
            rateManager.AddProvider(new FedExProvider(fedexKey, fedexPassword, fedexAccountNumber, fedexMeterNumber, fedexUseProduction));
            rateManager.AddProvider(new FedExSmartPostProvider(fedexKey, fedexPassword, fedexAccountNumber, fedexMeterNumber, fedexHubId, fedexUseProduction));
            rateManager.AddProvider(new USPSProvider(uspsUserId));
            rateManager.AddProvider(new USPSInternationalProvider(uspsUserId));

            // (Optional) Add RateAdjusters
            rateManager.AddRateAdjuster(new PercentageRateAdjuster(.9M));

            // Call GetRates()
            var shipment = rateManager.GetRates(origin, destination, packages);

            // Iterate through the rates returned
            foreach (var rate in shipment.Rates)
            {
                Console.WriteLine(rate);
            }

            Console.ReadLine();
        }
    }
}