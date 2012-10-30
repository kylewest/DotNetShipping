using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using DotNetShipping.ShippingProviders;

namespace DotNetShipping.SampleApp
{
	internal class Program
	{
		#region Methods

		private static void Main(string[] args)
		{
			NameValueCollection appSettings = ConfigurationManager.AppSettings;

			// You will need a license #, userid and password to utilize the UPS provider.
			string upsLicenseNumber = appSettings["UPSLicenseNumber"];
			string upsUserId = appSettings["UPSUserId"];
			string upsPassword = appSettings["UPSPassword"];

			// You will need an account # and meter # to utilize the FedEx provider.
			string fedexKey = appSettings["FedExKey"];
			string fedexPassword = appSettings["FedExPassword"];
			string fedexAccountNumber = appSettings["FedExAccountNumber"];
			string fedexMeterNumber = appSettings["FedExMeterNumber"];

			// You will need a userId and password to use the USPS provider. Your account will also need access to the production servers.
			string uspsUserId = appSettings["USPSUserId"];
			string uspsPassword = appSettings["USPSPassword"];

			// Setup package and destination/origin addresses
			var packages = new List<Package>();
			packages.Add(new Package(0, 0, 0, 35, 0));
			packages.Add(new Package(0, 0, 0, 15, 0));

			var origin = new Address("", "", "06405", "US");
			var destination = new Address("", "", "20852", "US"); // US Address
			//var destination = new Address("", "", "L4W 1S2", "CA"); // Canada Address
			//var destination = new Address("Bath", "", "BA11HX", "GB"); // European Address

			// Create RateManager
			var rateManager = new RateManager();

			// Add desired DotNetShippingProviders
			rateManager.AddProvider(new UPSProvider(upsLicenseNumber, upsUserId, upsPassword));
			rateManager.AddProvider(new FedExProvider(fedexKey, fedexPassword, fedexAccountNumber, fedexMeterNumber));
			rateManager.AddProvider(new USPSProvider(uspsUserId, uspsPassword));

			// (Optional) Add RateAdjusters
			rateManager.AddRateAdjuster(new PercentageRateAdjuster(.9M));

			// Call GetRates()
			Shipment shipment = rateManager.GetRates(origin, destination, packages);

			// Iterate through the rates returned
			foreach (Rate rate in shipment.Rates)
			{
				Console.WriteLine(rate);
			}
		}

		#endregion
	}
}