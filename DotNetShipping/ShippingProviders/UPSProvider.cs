using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Linq;
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

		private const string trackUrl = "https://www.ups.com/ups.app/xml/Track";
		// this is the test URL: "https://wwwcie.ups.com/ups.app/xml/Track"

		private readonly string _licenseNumber;
		private readonly string _password;
		private readonly Hashtable _serviceCodes = new Hashtable(12);
		private readonly int _timeout;
		private readonly string _userID;
		private AvailableServices _services = AvailableServices.All;

		#endregion

		#region .ctor

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
			var request = (HttpWebRequest) WebRequest.Create(ratesUrl);
			request.Method = "POST";
			request.Timeout = _timeout*1000;
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
			var response = (HttpWebResponse) request.GetResponse();
			parseRatesResponseMessage(new StreamReader(response.GetResponseStream()).ReadToEnd());
			response.Close();
		}

		public override Shipment GetTrackingActivity(string trackingNumber)
		{
			var shipment = new Shipment(trackingNumber);
			var request = (HttpWebRequest) WebRequest.Create(trackUrl);
			request.Method = "POST";
			request.ContentType = "application/x-www-form-urlencoded";
			request.Timeout = _timeout*1000;
			byte[] bytes = Encoding.ASCII.GetBytes(buildTrackingActivityRequestMessage(trackingNumber));
			request.ContentLength = bytes.Length;
			Stream stream = request.GetRequestStream();
			stream.Write(bytes, 0, bytes.Length);
			stream.Close();
			var response = (HttpWebResponse) request.GetResponse();
			parseTrackingActivityResponseMessage(ref shipment, new StreamReader(response.GetResponseStream()).ReadToEnd());
			response.Close();
			return shipment;
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
				writer.WriteElementString("Weight", Shipment.Packages[i].Weight.ToString());
				writer.WriteEndElement(); // </PackageWeight>
				writer.WriteStartElement("Dimensions");
				writer.WriteElementString("Length", Shipment.Packages[i].Length.ToString());
				writer.WriteElementString("Width", Shipment.Packages[i].Width.ToString());
				writer.WriteElementString("Height", Shipment.Packages[i].Height.ToString());
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

		private string buildTrackingActivityRequestMessage(string trackingNumber)
		{
			Debug.WriteLine("Building Request...", "UPS");
			string request = "";
			request += "<?xml version=\"1.0\"?>\r\n";
			request += "<AccessRequest xml:lang=\"en-US\">\r\n";
			request += "<AccessLicenseNumber>" + _licenseNumber + "</AccessLicenseNumber>\r\n";
			request += "<UserId>" + _userID + "</UserId>\r\n";
			request += "<Password>" + _password + "</Password>\r\n";
			request += "</AccessRequest>\r\n";
			request += "<?xml version=\"1.0\"?>";

			var xDoc = new XmlDocument();
			XmlNode xRoot = xDoc.AppendChild(xDoc.CreateElement("TrackRequest"));
			xRoot.Attributes.Append(xDoc.CreateAttribute("lang")).Value = "en-US";
			XmlNode xRequest = xRoot.AppendChild(xDoc.CreateElement("Request"));
			XmlNode xNode = xRequest.AppendChild(xDoc.CreateElement("TransactionReference"));
			xNode.AppendChild(xDoc.CreateElement("CustomerContext")).InnerText = "Tracking";
			xNode.AppendChild(xDoc.CreateElement("XpciVersion")).InnerText = "1.0001";
			xRequest.AppendChild(xDoc.CreateElement("RequestAction")).InnerText = "Track";
			xRequest.AppendChild(xDoc.CreateElement("RequestOption")).InnerText = "activity";

			string[] trackingNumberArray = trackingNumber.Split(new[] {','});
			foreach (string tracking in trackingNumberArray)
			{
				xRoot.AppendChild(xDoc.CreateElement("TrackingNumber")).InnerText = tracking;
			}

			request += xDoc.OuterXml;
			Debug.WriteLine(request);
			return request;
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
					service = (AvailableService) _serviceCodes[name];
				}
				else
				{
					continue;
				}
				if (((int) _services & service.EnumValue) != service.EnumValue)
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
				var rate = new Rate(Name, name, description, totalCharges, delivery);
				if (Shipment.RateAdjusters != null)
				{
					rate = Shipment.RateAdjusters.Aggregate(rate, (current, adjuster) => adjuster.AdjustRate(current));
				}
				Shipment.rates.Add(rate);
			}
		}

		private void parseTrackingActivityResponseMessage(ref Shipment shipment, string response)
		{
			Debug.WriteLine("UPS Response Received!");
			Debug.WriteLine(response);
			var xDoc = new XmlDocument();
			xDoc.LoadXml(response);
			XmlNodeList tracks = xDoc.SelectNodes("/TrackResponse/Shipment/Package");
			foreach (XmlNode track in tracks)
			{
				string trackingNumber = track.SelectSingleNode("TrackingNumber").InnerText;
				XmlNodeList activities = track.SelectNodes("Activity");
				foreach (XmlNode activity in activities)
				{
					string statusDescription = activity.SelectSingleNode("Status/StatusType/Description").InnerText;
					XmlNode nodeCity = activity.SelectSingleNode("ActivityLocation/Address/City");
					string city = (nodeCity == null ? "" : nodeCity.InnerText);
					XmlNode nodeState = activity.SelectSingleNode("ActivityLocation/Address/StateProvinceCode");
					string state = (nodeState == null ? "" : nodeState.InnerText);
					XmlNode nodeCountry = activity.SelectSingleNode("ActivityLocation/Address/CountryCode");
					string countryCode = (nodeCountry == null ? "" : nodeCountry.InnerText);
					string date = activity.SelectSingleNode("Date").InnerText;
					if (date != "")
					{
						date =
							new DateTime(Int16.Parse(date.Substring(0, 4)), Int16.Parse(date.Substring(4, 2)),
							             Int16.Parse(date.Substring(6, 2))).ToShortDateString();
					}
					string time = activity.SelectSingleNode("Time").InnerText;
					if (time == "")
					{
						time = new DateTime(1900, 1, 1, 11, 59, 59, 0).ToShortTimeString();
					}
					else
					{
						time =
							new DateTime(1900, 1, 1, Int16.Parse(time.Substring(0, 2)), Int16.Parse(time.Substring(2, 2)),
							             Int16.Parse(time.Substring(4, 2))).ToShortTimeString();
					}

					shipment.trackingActivities.Add(new TrackingActivity(trackingNumber, statusDescription, city, state, countryCode,
					                                                     date, time));
				}
			}
		}

		#endregion

		// These values need to stay in sync with the values in the "loadServiceCodes" method.

		#region AvailableServices enum

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

		#endregion

		#region Nested type: AvailableService

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

		#endregion
	}
}