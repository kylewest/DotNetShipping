# DotNetShipping

.NET wrapper to UPS, FedEx, and USPS APIs. Use it to retrieve shipping rates from these carriers.


## How to Install

Available in the [NuGet Gallery](http://nuget.org/packages/DotNetShipping):

```
PM> Install-Package DotNetShipping
```


## How to Build

```
git clone https://github.com/kylewest/DotNetShipping.git
cd DotNetShipping
./build.bat
```


## How to Use

```CSharp
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

// You will need a userId to use the USPS provider. Your account will also need access to the production servers.
string uspsUserId = appSettings["USPSUserId"];

// Setup package and destination/origin addresses
var packages = new List<Package>();
packages.Add(new Package(12, 12, 12, 35, 150));
packages.Add(new Package(4, 4, 6, 15, 250));

var origin = new Address("", "", "06405", "US");
var destination = new Address("", "", "20852", "US"); // US Address

// Create RateManager
var rateManager = new RateManager();

// Add desired DotNetShippingProviders
rateManager.AddProvider(new UPSProvider(upsLicenseNumber, upsUserId, upsPassword));
rateManager.AddProvider(new FedExProvider(fedexKey, fedexPassword, fedexAccountNumber, fedexMeterNumber));
rateManager.AddProvider(new USPSProvider(uspsUserId));

// (Optional) Add RateAdjusters
rateManager.AddRateAdjuster(new PercentageRateAdjuster(.9M));

// Call GetRates()
Shipment shipment = rateManager.GetRates(origin, destination, packages);

// Iterate through the rates returned
foreach (Rate rate in shipment.Rates)
{
	Console.WriteLine(rate);
}
```

See the sample app in this repository for a working example.


## 3rd Party Docs

Developer documentation is often hard to find. The links below are provided as reference.

* [FedEx](https://rpmware.box.com/s/e1i8dultmit9x16jo1m1)
* [USPS](https://rpmware.box.com/s/cvrmikfhnpm25r4qmb3f)
* [UPS] (https://www.dropbox.com/sh/p4w81e6xi6eycsd/ei-QZHL0vI)

## Credits & Contributors

Originally forked from [dotNETShipping](http://dotnetshipping.codeplex.com/) by [@rlaneve](https://github.com/rlaneve).

* [@kylewest](https://github.com/kylewest)
* [@brettallred](https://github.com/brettallred)

