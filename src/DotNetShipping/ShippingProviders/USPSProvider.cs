using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace DotNetShipping.ShippingProviders
{
	///<summary>
	///</summary>
	public class USPSProvider : AbstractShippingProvider
	{
		#region Fields

		private const string PRODUCTION_URL = "http://production.shippingapis.com/ShippingAPI.dll";
		private const string REMOVE_FROM_RATE_NAME = "&lt;sup&gt;&amp;reg;&lt;/sup&gt;";
		private readonly string _userId;

		#endregion

		#region .ctor

		///<summary>
		///</summary>
		///<param name="userId"></param>
		public USPSProvider(string userId)
		{
			Name = "USPS";
			_userId = userId;
		}

		#endregion

		#region Methods

		public override void GetRates()
		{
			var sb = new StringBuilder();

			var settings = new XmlWriterSettings();
			settings.Indent = false;
			settings.OmitXmlDeclaration = true;
			settings.NewLineHandling = NewLineHandling.None;

			using (XmlWriter writer = XmlWriter.Create(sb, settings))
			{
				writer.WriteStartElement("RateV4Request");
				writer.WriteAttributeString("USERID", _userId);
				int i = 0;
				foreach (Package package in Shipment.Packages)
				{
					writer.WriteStartElement("Package");
					writer.WriteAttributeString("ID", i.ToString());
					writer.WriteElementString("Service", "ALL");
					writer.WriteElementString("ZipOrigination", Shipment.OriginAddress.PostalCode);
					writer.WriteElementString("ZipDestination", Shipment.DestinationAddress.PostalCode);
					writer.WriteElementString("Pounds", package.RoundedWeight.ToString());
					writer.WriteElementString("Ounces", "0");
					writer.WriteElementString("Container", string.Empty);
					writer.WriteElementString("Size", "REGULAR");
					//TODO: Figure out DIM Weights
					//writer.WriteElementString("Size", package.IsOversize ? "LARGE" : "REGULAR");
					//writer.WriteElementString("Length", package.RoundedLength.ToString());
					//writer.WriteElementString("Width", package.RoundedWidth.ToString());
					//writer.WriteElementString("Height", package.RoundedHeight.ToString());
					//writer.WriteElementString("Girth", package.CalculatedGirth.ToString());
					writer.WriteElementString("Machinable", "True");
					writer.WriteEndElement();
					i++;
				}
				writer.WriteEndElement();
				writer.Flush();
			}

			try
			{
				string url = string.Concat(PRODUCTION_URL, "?API=RateV4&XML=", sb.ToString());
				var webClient = new WebClient();
				string response = webClient.DownloadString(url);

				Debug.WriteLine(url);
				Debug.WriteLine(response);

				ParseResult(response);
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex);
			}
		}

		private void ParseResult(string response)
		{
			XElement document = XElement.Parse(response, LoadOptions.None);

			var rates = from item in document.Descendants("Postage")
			            group item by (string) item.Element("MailService")
			            into g select new {Name = g.Key, TotalCharges = g.Sum(x => Decimal.Parse((string) x.Element("Rate")))};

			foreach (var r in rates)
			{
				string name = r.Name.Replace(REMOVE_FROM_RATE_NAME, string.Empty);

				AddRate(name, string.Concat("USPS ", name), r.TotalCharges, DateTime.Now.AddDays(30));
			}
		}

		#endregion
	}
}