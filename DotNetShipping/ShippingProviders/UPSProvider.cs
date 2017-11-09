using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace DotNetShipping.ShippingProviders
{
    /// <summary>
    ///     Provides rates from UPS (United Parcel Service).
    /// </summary>
    public class UPSProvider : AbstractShippingProvider
    {
        // These values need to stay in sync with the values in the "loadServiceCodes" method.

        public enum AvailableServices
        {
            NextDayAir = 1,
            SecondDayAir = 2,
            Ground = 4,
            WorldwideExpress = 8,
            WorldwideExpedited = 16,
            Standard = 32,
            ThreeDaySelect = 64,
            NextDayAirSaver = 128,
            NextDayAirEarlyAM = 256,
            WorldwideExpressPlus = 512,
            SecondDayAirAM = 1024,
            ExpressSaver = 2048,
            SurePost = 4096,
            All = 8191
        }
        private const int DEFAULT_TIMEOUT = 10;
        private const string DEVELOPMENT_RATES_URL = "https://wwwcie.ups.com/ups.app/xml/Rate";
        private const string PRODUCTION_RATES_URL = "https://onlinetools.ups.com/ups.app/xml/Rate";
        private AvailableServices _services = AvailableServices.All;
        private bool _useNegotiatedRates = false;
        private bool _useRetailRates = false;
        private bool _useProduction = true;
        private readonly string _licenseNumber;
        private readonly string _password;
        private readonly Hashtable _serviceCodes = new Hashtable(12);
        private readonly string _serviceDescription;
        private readonly string _shipperNumber;
        private readonly int _timeout;
        private readonly string _userId;

        /// <summary>
        ///     Parameterless construction that pulls data directly from app.config
        /// </summary>
        public UPSProvider()
        {
            Name = "UPS";
            var appSettings = ConfigurationManager.AppSettings;
            _licenseNumber = appSettings["UPSLicenseNumber"];
            _userId = appSettings["UPSUserId"];
            _password = appSettings["UPSPassword"];
            _timeout = DEFAULT_TIMEOUT;
            _serviceDescription = "";
            LoadServiceCodes();
            _shipperNumber = "";
        }

        public UPSProvider(string licenseNumber, string userId, string password) : this(licenseNumber, userId, password, DEFAULT_TIMEOUT)
        {
        }

        public UPSProvider(string licenseNumber, string userId, string password, int timeout)
        {
            Name = "UPS";
            _licenseNumber = licenseNumber;
            _userId = userId;
            _password = password;
            _timeout = timeout;
            _serviceDescription = "";
            _shipperNumber = "";
            LoadServiceCodes();
        }

        public UPSProvider(string licenseNumber, string userId, string password, string serviceDescription)
        {
            Name = "UPS";
            _licenseNumber = licenseNumber;
            _userId = userId;
            _password = password;
            _timeout = DEFAULT_TIMEOUT;
            _serviceDescription = serviceDescription;
            _shipperNumber = "";
            LoadServiceCodes();
        }

        public UPSProvider(string licenseNumber, string userId, string password, int timeout, string serviceDescription)
        {
            Name = "UPS";
            _licenseNumber = licenseNumber;
            _userId = userId;
            _password = password;
            _timeout = timeout;
            _serviceDescription = serviceDescription;
            _shipperNumber = "";
            LoadServiceCodes();
        }

        public UPSProvider(string licenseNumber, string userId, string password, int timeout, string serviceDescription, string shipperNumber)
        {
             Name = "UPS";
             _licenseNumber = licenseNumber;
             _userId = userId;
             _password = password;
             _timeout = timeout;
             _serviceDescription = serviceDescription;
             _shipperNumber = shipperNumber;
             LoadServiceCodes();
        }

        public AvailableServices Services
        {
            get { return _services; }
            set { _services = value; }
        }

        private string RatesUrl => UseProduction ? PRODUCTION_RATES_URL : DEVELOPMENT_RATES_URL;

        public bool UseRetailRates
        {
            get { return _useRetailRates; }
            set { _useRetailRates = value; }
        }

        public bool UseNegotiatedRates
        {
             get { return _useNegotiatedRates; }
             set { _useNegotiatedRates = value; }
        }

        public bool UseProduction
        {
            get { return _useProduction; }
            set { _useProduction = value; }
        }

        private byte[] BuildRatesRequestMessage()
        {
            Encoding utf8 = new UTF8Encoding(false);
            var writer = new XmlTextWriter(new MemoryStream(2000), utf8);
            writer.WriteStartDocument();
            writer.WriteStartElement("AccessRequest");
            writer.WriteAttributeString("lang", "en-US");
            writer.WriteElementString("AccessLicenseNumber", _licenseNumber);
            writer.WriteElementString("UserId", _userId);
            writer.WriteElementString("Password", _password);
            writer.WriteEndDocument();
            writer.WriteStartDocument();
            writer.WriteStartElement("RatingServiceSelectionRequest");
            writer.WriteAttributeString("lang", "en-US");
            writer.WriteStartElement("Request");
            writer.WriteStartElement("TransactionReference");
            writer.WriteElementString("CustomerContext", "Rating and Service");
            writer.WriteElementString("XpciVersion", "1.0");
            writer.WriteEndElement(); // </TransactionReference>
            writer.WriteElementString("RequestAction", "Rate");
            writer.WriteElementString("RequestOption", string.IsNullOrWhiteSpace(_serviceDescription) ? "Shop" : _serviceDescription);
            writer.WriteEndElement(); // </Request>
            writer.WriteStartElement("PickupType");
            writer.WriteElementString("Code", "03");
            writer.WriteEndElement(); // </PickupType>
            writer.WriteStartElement("CustomerClassification");

            if (_useRetailRates)
            {
                writer.WriteElementString("Code", "04"); //04 gets retail rates
            }
            else
            {
                writer.WriteElementString("Code", string.IsNullOrWhiteSpace(_shipperNumber) ? "01" : "00"); // 00 gets shipper number rates, 01 for daily rates
            }

            writer.WriteEndElement(); // </CustomerClassification
            writer.WriteStartElement("Shipment");
            writer.WriteStartElement("Shipper");
            if (!string.IsNullOrWhiteSpace(_shipperNumber))
            {
                writer.WriteElementString("ShipperNumber", _shipperNumber);
            }
            writer.WriteStartElement("Address");
            if (!string.IsNullOrWhiteSpace(Shipment.OriginAddress.State))
            {
                writer.WriteElementString("StateProvinceCode", Shipment.OriginAddress.State);
            }
            writer.WriteElementString("PostalCode", Shipment.OriginAddress.PostalCode);
            writer.WriteElementString("CountryCode", Shipment.OriginAddress.CountryCode);
            writer.WriteEndElement(); // </Address>
            writer.WriteEndElement(); // </Shipper>
            writer.WriteStartElement("ShipTo");
            writer.WriteStartElement("Address");
            if (!string.IsNullOrWhiteSpace(Shipment.DestinationAddress.State))
            {
                writer.WriteElementString("StateProvinceCode", Shipment.DestinationAddress.State);
            }
			if (!string.IsNullOrWhiteSpace(Shipment.DestinationAddress.PostalCode))
			{
                writer.WriteElementString("PostalCode", Shipment.DestinationAddress.PostalCode);
            }
            writer.WriteElementString("CountryCode", Shipment.DestinationAddress.CountryCode);
			if (Shipment.DestinationAddress.IsResidential)
			{
				writer.WriteElementString("ResidentialAddressIndicator", "true");
			}
			writer.WriteEndElement(); // </Address>
            writer.WriteEndElement(); // </ShipTo>
            if (!string.IsNullOrWhiteSpace(_serviceDescription))
            {
                writer.WriteStartElement("Service");
                writer.WriteElementString("Code", _serviceDescription.ToUpsShipCode());
                writer.WriteEndElement(); //</Service>
            }
            if (_useNegotiatedRates)
            {
                writer.WriteStartElement("RateInformation");
                writer.WriteElementString("NegotiatedRatesIndicator", "");
                writer.WriteEndElement();// </RateInformation>
            }
            for (var i = 0; i < Shipment.Packages.Count; i++)
            {
                writer.WriteStartElement("Package");
                writer.WriteStartElement("PackagingType");
                writer.WriteElementString("Code", "02");
                writer.WriteEndElement(); //</PackagingType>
                writer.WriteStartElement("PackageWeight");
                writer.WriteElementString("Weight", Shipment.Packages[i].RoundedWeight.ToString());
                writer.WriteEndElement(); // </PackageWeight>
                writer.WriteStartElement("Dimensions");
                writer.WriteElementString("Length", Shipment.Packages[i].RoundedLength.ToString());
                writer.WriteElementString("Width", Shipment.Packages[i].RoundedWidth.ToString());
                writer.WriteElementString("Height", Shipment.Packages[i].RoundedHeight.ToString());
                writer.WriteEndElement(); // </Dimensions>
                writer.WriteStartElement("PackageServiceOptions");
                writer.WriteStartElement("InsuredValue");
                writer.WriteElementString("CurrencyCode", "USD");
                writer.WriteElementString("MonetaryValue", Shipment.Packages[i].InsuredValue.ToString());
                writer.WriteEndElement(); // </InsuredValue>

                if (Shipment.Packages[i].SignatureRequiredOnDelivery)
                {
                    writer.WriteStartElement("DeliveryConfirmation");
                    writer.WriteElementString("DCISType", "2");         // 2 represents Delivery Confirmation Signature Required
                    writer.WriteEndElement(); // </DeliveryConfirmation>
                }

                writer.WriteEndElement(); // </PackageServiceOptions>
                writer.WriteEndElement(); // </Package>
            }
            writer.WriteEndDocument();
            writer.Flush();
            var buffer = new byte[writer.BaseStream.Length];
            writer.BaseStream.Position = 0;
            writer.BaseStream.Read(buffer, 0, buffer.Length);
            writer.Close();

            return buffer;
        }

        public override void GetRates()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

            var request = (HttpWebRequest) WebRequest.Create(RatesUrl);
            request.Method = "POST";
            request.Timeout = _timeout * 1000;
            // Per the UPS documentation, the "ContentType" should be "application/x-www-form-urlencoded".
            // However, using "text/xml; encoding=UTF-8" lets us avoid converting the byte array returned by
            // the buildRatesRequestMessage method and (so far) works just fine.
            request.ContentType = "text/xml; encoding=UTF-8"; //"application/x-www-form-urlencoded";
            var bytes = BuildRatesRequestMessage();
            //System.Text.Encoding.Convert(Encoding.UTF8, Encoding.ASCII, this.buildRatesRequestMessage());
            request.ContentLength = bytes.Length;
            var stream = request.GetRequestStream();
            stream.Write(bytes, 0, bytes.Length);
            stream.Close();

            using (var resp = request.GetResponse() as HttpWebResponse)
            {
                if (resp != null && resp.StatusCode == HttpStatusCode.OK)
                {
                    var xDoc = XDocument.Load(resp.GetResponseStream());
                    ParseRatesResponseMessage(xDoc);
                }
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public IDictionary<string, string> GetServiceCodes()
        {
            if (_serviceCodes != null && _serviceCodes.Count > 0)
            {
                var serviceCodes = new Dictionary<string, string>();

                foreach (var serviceCodeKey in _serviceCodes.Keys)
                {
                    var serviceCode = (AvailableService)_serviceCodes[serviceCodeKey];
                    serviceCodes.Add((string)serviceCodeKey, serviceCode.Name);
                }

                return serviceCodes;
            }

            return null;
        }

        private void LoadServiceCodes()
        {
            _serviceCodes.Add("01", new AvailableService("UPS Next Day Air", 1));
            _serviceCodes.Add("02", new AvailableService("UPS Second Day Air", 2));
            _serviceCodes.Add("03", new AvailableService("UPS Ground", 4));
            _serviceCodes.Add("07", new AvailableService("UPS Worldwide Express", 8));
            _serviceCodes.Add("08", new AvailableService("UPS Worldwide Expedited", 16));
            _serviceCodes.Add("11", new AvailableService("UPS Standard", 32));
            _serviceCodes.Add("12", new AvailableService("UPS 3-Day Select", 64));
            _serviceCodes.Add("13", new AvailableService("UPS Next Day Air Saver", 128));
            _serviceCodes.Add("14", new AvailableService("UPS Next Day Air Early AM", 256));
            _serviceCodes.Add("54", new AvailableService("UPS Worldwide Express Plus", 512));
            _serviceCodes.Add("59", new AvailableService("UPS 2nd Day Air AM", 1024));
            _serviceCodes.Add("65", new AvailableService("UPS Express Saver", 2048));
            _serviceCodes.Add("93", new AvailableService("UPS Sure Post", 4096));
        }

        private void ParseRatesResponseMessage(XDocument xDoc)
        {
            if (xDoc.Root != null)
            {
                var ratedShipment = xDoc.Root.Elements("RatedShipment");
                foreach (var rateNode in ratedShipment)
                {
                    var name = rateNode.XPathSelectElement("Service/Code").Value;
                    AvailableService service;
                    if (_serviceCodes.ContainsKey(name))
                    {
                        service = (AvailableService) _serviceCodes[name];
                    }
                    else
                    {
                        continue;
                    }
                    if (((int) Services & service.EnumValue) != service.EnumValue)
                    {
                        continue;
                    }
                    var description = "";
                    if (_serviceCodes.ContainsKey(name))
                    {
                        description = _serviceCodes[name].ToString();
                    }
                    var totalCharges = Convert.ToDecimal(rateNode.XPathSelectElement("TotalCharges/MonetaryValue").Value);
                    if (_useNegotiatedRates)
                    {
                        var negotiatedRate = rateNode.XPathSelectElement("NegotiatedRates/NetSummaryCharges/GrandTotal/MonetaryValue");
                        if (negotiatedRate != null) // check for negotiated rate
                        {
                            totalCharges = Convert.ToDecimal(negotiatedRate.Value);
                        }
                    }
                    var delivery = DateTime.Parse("1/1/1900 12:00 AM");
                    var date = rateNode.XPathSelectElement("GuaranteedDaysToDelivery").Value;
                    if (date == "") // no gauranteed delivery date, so use MaxDate to ensure correct sorting
                    {
                        date = DateTime.MaxValue.ToShortDateString();
                    }
                    else
                    {
                        date = DateTime.Now.AddDays(Convert.ToDouble(date)).ToShortDateString();
                    }
                    var deliveryTime = rateNode.XPathSelectElement("ScheduledDeliveryTime").Value;
                    if (deliveryTime == "") // no scheduled delivery time, so use 11:59:00 PM to ensure correct sorting
                    {
                        date += " 11:59:00 PM";
                    }
                    else
                    {
                        date += " " + deliveryTime.Replace("Noon", "PM").Replace("P.M.", "PM").Replace("A.M.", "AM");
                    }
                    if (date != "")
                    {
                        delivery = DateTime.Parse(date);
                    }

                    AddRate(name, description, totalCharges, delivery);
                }
            }
        }

        private struct AvailableService
        {
            public readonly int EnumValue;
            public readonly string Name;

            public AvailableService(string name, int enumValue)
            {
                Name = name;
                EnumValue = enumValue;
            }

            public override string ToString()
            {
                return Name;
            }
        }
    }
}
