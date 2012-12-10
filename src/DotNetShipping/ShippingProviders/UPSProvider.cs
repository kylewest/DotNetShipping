using System;
using System.Collections;
using System.Collections.Specialized;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Xml;

namespace DotNetShipping.ShippingProviders
{
    /// <summary>
    /// 	Provides rates from UPS (United Parcel Service).
    /// </summary>
    public class UPSProvider : AbstractShippingProvider
    {
        #region Fields

        private const int defaultTimeout = 10;
        private const string ratesUrl = "https://www.ups.com/ups.app/xml/Rate";

        private readonly string _licenseNumber;
        private readonly string _password;
        private readonly Hashtable _serviceCodes = new Hashtable(12);
        private readonly int _timeout;
        private readonly string _userID;
        private AvailableServices _services = AvailableServices.All;

        #endregion

        #region .ctor

        /// <summary>
        /// Parameterless construction that pulls data directly from app.config
        /// </summary>
        public UPSProvider()
        {
            NameValueCollection appSettings = ConfigurationManager.AppSettings;
            _licenseNumber = appSettings["UPSLicenseNumber"];
            _userID = appSettings["UPSUserId"];
            _password = appSettings["UPSPassword"];
            _timeout = defaultTimeout;
        }

        public UPSProvider(string licenseNumber, string userID, string password)
            : this(licenseNumber, userID, password, defaultTimeout)
        {
        }

        public UPSProvider(string licenseNumber, string userID, string password, int timeout)
        {
            Name = "UPS";
            _licenseNumber = licenseNumber;
            _userID = userID;
            _password = password;
            _timeout = timeout;

            loadServiceCodes();
        }

        #endregion

        #region Properties

        public AvailableServices Services
        {
            get { return _services; }
            set { _services = value; }
        }

        #endregion

        #region Methods

        public override void GetRates()
        {
            var request = (HttpWebRequest)WebRequest.Create(ratesUrl);
            request.Method = "POST";
            request.Timeout = _timeout * 1000;
            // Per the UPS documentation, the "ContentType" should be "application/x-www-form-urlencoded".
            // However, using "text/xml; encoding=UTF-8" lets us avoid converting the byte array returned by
            // the buildRatesRequestMessage method and (so far) works just fine.
            request.ContentType = "text/xml; encoding=UTF-8"; //"application/x-www-form-urlencoded";
            byte[] bytes = buildRatesRequestMessage();
            //System.Text.Encoding.Convert(Encoding.UTF8, Encoding.ASCII, this.buildRatesRequestMessage());
            request.ContentLength = bytes.Length;
            Stream stream = request.GetRequestStream();
            stream.Write(bytes, 0, bytes.Length);
            stream.Close();
            Debug.WriteLine("Request Sent!", "UPS");
            var response = (HttpWebResponse)request.GetResponse();
            parseRatesResponseMessage(new StreamReader(response.GetResponseStream()).ReadToEnd());
            response.Close();
        }

        private byte[] buildRatesRequestMessage()
        {
            Debug.WriteLine("Building Request...", "UPS");

            Encoding utf8 = new UTF8Encoding(false);
            var writer = new XmlTextWriter(new MemoryStream(2000), utf8);
            writer.WriteStartDocument();
            writer.WriteStartElement("AccessRequest");
            writer.WriteAttributeString("lang", "en-US");
            writer.WriteElementString("AccessLicenseNumber", _licenseNumber);
            writer.WriteElementString("UserId", _userID);
            writer.WriteElementString("Password", _password);
            writer.WriteEndDocument();
            writer.WriteStartDocument();
            writer.WriteStartElement("RatingServiceSelectionRequest");
            writer.WriteAttributeString("lang", "en-US");
            writer.WriteStartElement("Request");
            writer.WriteStartElement("TransactionReference");
            writer.WriteElementString("CustomerContext", "Rating and Service");
            writer.WriteElementString("XpciVersion", "1.0001");
            writer.WriteEndElement(); // </TransactionReference>
            writer.WriteElementString("RequestAction", "Rate");
            writer.WriteElementString("RequestOption", "Shop");
            writer.WriteEndElement(); // </Request>
            writer.WriteStartElement("PickupType");
            writer.WriteElementString("Code", "03");
            writer.WriteEndElement(); // </PickupType>
            writer.WriteStartElement("Shipment");
            writer.WriteStartElement("Shipper");
            writer.WriteStartElement("Address");
            writer.WriteElementString("PostalCode", Shipment.OriginAddress.PostalCode);
            writer.WriteEndElement(); // </Address>
            writer.WriteEndElement(); // </Shipper>
            writer.WriteStartElement("ShipTo");
            writer.WriteStartElement("Address");
            writer.WriteElementString("PostalCode", Shipment.DestinationAddress.PostalCode);
            writer.WriteElementString("CountryCode", Shipment.DestinationAddress.CountryCode);
            writer.WriteEndElement(); // </Address>
            writer.WriteEndElement(); // </ShipTo>
            for (int i = 0; i < Shipment.Packages.Count; i++)
            {
                writer.WriteStartElement("Package");
                writer.WriteStartElement("PackagingType");
                writer.WriteElementString("Code", "00");
                writer.WriteEndElement(); //</PackagingType>
                writer.WriteStartElement("PackageWeight");
                writer.WriteElementString("Weight", Shipment.Packages[i].RoundedWeight.ToString());
                writer.WriteEndElement(); // </PackageWeight>
                writer.WriteStartElement("Dimensions");
                writer.WriteElementString("Length", Shipment.Packages[i].RoundedLength.ToString());
                writer.WriteElementString("Width", Shipment.Packages[i].RoundedWidth.ToString());
                writer.WriteElementString("Height", Shipment.Packages[i].RoundedHeight.ToString());
                writer.WriteEndElement(); // </Dimensions>
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

        private void loadServiceCodes()
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
        }

        private void parseRatesResponseMessage(string response)
        {
            Debug.WriteLine("UPS Response Received!");
            Debug.WriteLine(response);
            var xDoc = new XmlDocument();
            xDoc.LoadXml(response);
            XmlNodeList ratedShipment = xDoc.SelectNodes("/RatingServiceSelectionResponse/RatedShipment");
            foreach (XmlNode rateNode in ratedShipment)
            {
                string name = rateNode.SelectSingleNode("Service/Code").InnerText;
                AvailableService service;
                if (_serviceCodes.ContainsKey(name))
                {
                    service = (AvailableService)_serviceCodes[name];
                }
                else
                {
                    continue;
                }
                if (((int)_services & service.EnumValue) != service.EnumValue)
                {
                    continue;
                }
                string description = "";
                if (_serviceCodes.ContainsKey(name))
                {
                    description = _serviceCodes[name].ToString();
                }
                decimal totalCharges = Convert.ToDecimal(rateNode.SelectSingleNode("TotalCharges/MonetaryValue").InnerText);
                DateTime delivery = DateTime.Parse("1/1/1900 12:00 AM");
                string date = rateNode.SelectSingleNode("GuaranteedDaysToDelivery").InnerText;
                if (date == "") // no gauranteed delivery date, so use MaxDate to ensure correct sorting
                {
                    date = DateTime.MaxValue.ToShortDateString();
                }
                else
                {
                    date = DateTime.Now.AddDays(Convert.ToDouble(date)).ToShortDateString();
                }
                string deliveryTime = rateNode.SelectSingleNode("ScheduledDeliveryTime").InnerText;
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

        #endregion

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
            All = 4095
        }

        private struct AvailableService
        {
            #region Fields

            public readonly int EnumValue;
            public readonly string Name;

            #endregion

            #region .ctor

            public AvailableService(string name, int enumValue)
            {
                Name = name;
                EnumValue = enumValue;
            }

            #endregion

            #region Methods

            public override string ToString()
            {
                return Name;
            }

            #endregion
        }
    }
}