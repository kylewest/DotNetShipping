using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Xml;

namespace DotNetShipping.ShippingProviders
{
	/// <summary>
	/// 	Provides rates from FedEx (Federal Express).
	/// </summary>
	public class FedExProvider : AbstractShippingProvider
	{
		#region Fields

		private const int DEFAULT_TIMEOUT = 10;
		private const string URL = "https://gateway.fedex.com/GatewayDC";

		private readonly string _accountNumber;
		private readonly string _meterNumber;

		private readonly Hashtable _serviceCodes = new Hashtable
		                                           	{
		                                           		{
		                                           			"PRIORITYOVERNIGHT", new AvailableService("FedEx Priority Overnight", 1)
		                                           			},
		                                           		{"FEDEX2DAY", new AvailableService("FedEx 2nd Day", 2)},
		                                           		{
		                                           			"STANDARDOVERNIGHT", new AvailableService("FedEx Standard Overnight", 4)
		                                           			},
		                                           		{"FIRSTOVERNIGHT", new AvailableService("FedEx First Overnight", 8)},
		                                           		{"FEDEXEXPRESSSAVER", new AvailableService("FedEx Express Saver", 16)},
		                                           		{"FEDEX1DAYFREIGHT", new AvailableService("FedEx Overnight Freight", 32)},
		                                           		{"FEDEX2DAYFREIGHT", new AvailableService("FedEx 2nd Day Freight", 64)},
		                                           		{
		                                           			"FEDEX3DAYFREIGHT",
		                                           			new AvailableService("FedEx Express Saver Freight", 128)
		                                           			},
		                                           		{"GROUNDHOMEDELIVERY", new AvailableService("FedEx Home Delivery", 256)},
		                                           		{"FEDEXGROUND", new AvailableService("FedEx Ground", 512)},
		                                           		{
		                                           			"INTERNATIONALECONOMY",
		                                           			new AvailableService("FedEx International Economy", 9990)
		                                           			},
		                                           		{
		                                           			"INTERNATIONALPRIORITY",
		                                           			new AvailableService("FedEx International Priority", 9991)
		                                           			}
		                                           	};

		private readonly int _timeout;

		#endregion

		#region .ctor

		///<summary>
		///</summary>
		///<param name = "accountNumber"></param>
		///<param name = "meterNumber"></param>
		public FedExProvider(string accountNumber, string meterNumber) : this(accountNumber, meterNumber, DEFAULT_TIMEOUT)
		{
		}

		///<summary>
		///</summary>
		///<param name = "accountNumber"></param>
		///<param name = "meterNumber"></param>
		///<param name = "timeout"></param>
		public FedExProvider(string accountNumber, string meterNumber, int timeout)
		{
			_name = "FedEx";
			_accountNumber = accountNumber;
			_meterNumber = meterNumber;
			_timeout = timeout;
		}

		#endregion

		#region Methods

		public override void GetRates()
		{
			var request = (HttpWebRequest) WebRequest.Create(URL);
			request.Method = "POST";
			request.Timeout = _timeout*1000;
			// Per the FedEx documentation, the "ContentType" should be "application/x-www-form-urlencoded".
			// However, using "text/xml; encoding=UTF-8" lets us avoid converting the byte array returned by
			// the buildRatesRequestMessage method and (so far) works just fine.
			request.ContentType = "text/xml; encoding=UTF-8"; //"application/x-www-form-urlencoded";
			byte[] bytes = BuildRequestMessage();
			//System.Text.Encoding.Convert(Encoding.UTF8, Encoding.ASCII, this.buildRequestMessage());
			request.ContentLength = bytes.Length;
			Stream stream = request.GetRequestStream();
			stream.Write(bytes, 0, bytes.Length);
			stream.Close();
			Debug.WriteLine("Request Sent!", "FedEx");
			var response = (HttpWebResponse) request.GetResponse();
			var xml = new XmlDocument();
			xml.LoadXml(new StreamReader(response.GetResponseStream()).ReadToEnd());
			_shipment.rates.AddRange(ParseResponseMessage(xml));
			response.Close();
		}

		internal List<Rate> ParseResponseMessage(XmlDocument response)
		{
			Debug.WriteLine(response.OuterXml);
			var rates = new List<Rate>();

			XmlNodeList nodesEntries = response.SelectNodes("/FDXRateAvailableServicesReply/Entry");
			if (nodesEntries != null)
			{
				foreach (XmlNode nodeEntry in nodesEntries)
				{
					string rateName = nodeEntry.SelectSingleNode("Service").InnerText;
					if (!WasSeriveRequested(rateName))
					{
						continue;
					}
					string rateDesc = _serviceCodes[rateName].ToString();
					XmlNode rateNode = (ApplyDiscounts
					                    	? nodeEntry.SelectSingleNode("EstimatedCharges/DiscountedCharges/NetCharge")
					                    	: nodeEntry.SelectSingleNode("EstimatedCharges/ListCharges/NetCharge"));
					decimal totalCharges = Decimal.Parse(rateNode.InnerText);
					DateTime deliveryDate = DateTime.Now;
					if (nodeEntry.SelectSingleNode("DeliveryDate") != null)
					{
						deliveryDate = GetDeliveryDateTime(rateName, DateTime.Parse(nodeEntry.SelectSingleNode("DeliveryDate").InnerText));
					}
					else if (nodeEntry.SelectSingleNode("TimeInTransit") != null)
					{
						deliveryDate = GetDeliveryDateTime(rateName,
						                                   DateTime.Parse(
						                                   	DateTime.Now.AddDays(
						                                   		Convert.ToDouble(nodeEntry.SelectSingleNode("TimeInTransit").InnerText)).
						                                   		ToString("MM/dd/yyyy")));
					}
					rates.Add(new Rate(Name, rateName, rateDesc, totalCharges, deliveryDate));
				}
			}
			return rates;
		}

		private static DateTime GetDeliveryDateTime(string serviceCode, DateTime deliveryDate)
		{
			DateTime result = deliveryDate;

			switch (serviceCode)
			{
				case "PRIORITYOVERNIGHT":
					result = result.AddHours(10.5);
					break;
				case "FIRSTOVERNIGHT":
					result = result.AddHours(8.5);
					break;
				case "STANDARDOVERNIGHT":
					result = result.AddHours(15);
					break;
				case "FEDEX2DAY":
				case "FEDEXEXPRESSSAVER":
					result = result.AddHours(16.5);
					break;
				default: // no specific time, so use 11:59 PM to ensure correct sorting
					result = result.AddHours(23).AddMinutes(59);
					break;
			}
			return result;
		}

		private byte[] BuildRequestMessage()
		{
			Debug.WriteLine("Building Request...", "FedEx");

			Encoding utf8 = new UTF8Encoding(false);
			var writer = new XmlTextWriter(new MemoryStream(2000), utf8);
			writer.WriteStartDocument();
			writer.WriteStartElement("FDXRateAvailableServicesRequest");
			writer.WriteAttributeString("xmlns:api", "http://www.fedex.com/fsmapi");
			writer.WriteAttributeString("xmlns:xsi", "http://www.w3.org/2001/XMLSchema-instance");
			writer.WriteAttributeString("xsi:noNamespaceSchemaLocation", "FDXRateRequest.xsd");
			writer.WriteStartElement("RequestHeader");
			writer.WriteElementString("CustomerTransactionIdentifier", "RateRequest");
			writer.WriteElementString("AccountNumber", _accountNumber);
			writer.WriteElementString("MeterNumber", _meterNumber);
			writer.WriteEndElement();
			writer.WriteElementString("DropoffType", "REGULARPICKUP");
			writer.WriteElementString("Packaging", "YOURPACKAGING");
			writer.WriteElementString("WeightUnits", "LBS");
			writer.WriteElementString("Weight", _shipment.Packages[0].Weight.ToString("00.0"));
			writer.WriteElementString("ListRate", "1");
			writer.WriteStartElement("OriginAddress");
			writer.WriteElementString("StateOrProvinceCode", _shipment.OriginAddress.State);
			writer.WriteElementString("PostalCode", _shipment.OriginAddress.PostalCode);
			writer.WriteElementString("CountryCode", _shipment.OriginAddress.CountryCode);
			writer.WriteEndElement();
			writer.WriteStartElement("DestinationAddress");
			writer.WriteElementString("StateOrProvinceCode", _shipment.DestinationAddress.State);
			writer.WriteElementString("PostalCode", _shipment.DestinationAddress.PostalCode);
			writer.WriteElementString("CountryCode", _shipment.DestinationAddress.CountryCode);
			writer.WriteEndElement();
			writer.WriteStartElement("Payment");
			writer.WriteElementString("PayorType", "SENDER");
			writer.WriteEndElement();
			writer.WriteStartElement("Dimensions");
			writer.WriteElementString("Units", "IN");
			writer.WriteElementString("Length", _shipment.Packages[0].Length.ToString());
			writer.WriteElementString("Width", _shipment.Packages[0].Width.ToString());
			writer.WriteElementString("Height", _shipment.Packages[0].Height.ToString());
			writer.WriteEndElement();
			writer.WriteElementString("PackageCount", "1");
			writer.WriteEndDocument();

			writer.Flush();
			var buffer = new byte[writer.BaseStream.Length];
			writer.BaseStream.Position = 0;
			writer.BaseStream.Read(buffer, 0, buffer.Length);
			writer.Close();

			return buffer;
		}

		private bool WasSeriveRequested(string rateName)
		{
			return _serviceCodes.ContainsKey(rateName);
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