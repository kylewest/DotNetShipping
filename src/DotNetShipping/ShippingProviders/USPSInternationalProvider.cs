using System;
using System.Configuration;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;

namespace DotNetShipping.ShippingProviders
{
	///<summary>
	///</summary>
	public class USPSInternationalProvider : AbstractShippingProvider
	{
		#region Fields

		private const string PRODUCTION_URL = "http://production.shippingapis.com/ShippingAPI.dll";
		private readonly string _userId;
	    private readonly string _service;
        public bool Commercial { get; set; }

		#endregion

		#region .ctor

		public USPSInternationalProvider()
		{
			Name = "USPS";
			_userId = ConfigurationManager.AppSettings["USPSUserId"];
		    _service = "ALL";
		}

        ///<summary>
        ///</summary>
        ///<param name="userId"></param>
        public USPSInternationalProvider(string userId)
        {
            Name = "USPS";
            _userId = userId;
            _service = "ALL";
        }

		///<summary>
		///</summary>
		///<param name="userId"></param>
        public USPSInternationalProvider(string userId, string service)
		{
			Name = "USPS";
			_userId = userId;
		    _service = service;
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
                writer.WriteStartElement("IntlRateV2Request");
				writer.WriteAttributeString("USERID", _userId);

                writer.WriteElementString("Revision", "2");
				int i = 0;
				foreach (Package package in Shipment.Packages)
				{
                      //<Package ID="2ND">
                      //  <Pounds>0</Pounds>
                      //  <Ounces>3</Ounces>
                      //  <MailType>Envelope</MailType>
                      //  <ValueOfContents>750</ValueOfContents>
                      //  <Country>Algeria</Country>
                      //  <Container></Container>
                      //  <Size>REGULAR</Size>
                      //  <Width></Width>
                      //  <Length></Length>
                      //  <Height></Height>
                      //  <Girth></Girth>
                      //  <CommercialFlag>N</CommercialFlag>
                      //</Package>

					writer.WriteStartElement("Package");
					writer.WriteAttributeString("ID", i.ToString());
					writer.WriteElementString("Pounds", package.RoundedWeight.ToString());
                    writer.WriteElementString("Ounces", "0");
                    writer.WriteElementString("MailType", "Package");
                    writer.WriteElementString("ValueOfContents", package.InsuredValue < 0 ? package.InsuredValue.ToString() : "100"); //todo: figure out best way to come up with insured value
                    writer.WriteElementString("Country", Shipment.DestinationAddress.CountryCode);
					writer.WriteElementString("Container", "RECTANGULAR");
                    writer.WriteElementString("Size", "REGULAR");
				    writer.WriteElementString("Width", package.RoundedWidth.ToString());
				    writer.WriteElementString("Length", package.RoundedLength.ToString());
                    writer.WriteElementString("Height", package.RoundedHeight.ToString());
                    writer.WriteElementString("Girth", package.CalculatedGirth.ToString());
                    writer.WriteElementString("OriginZip", Shipment.OriginAddress.PostalCode);
				    writer.WriteElementString("CommercialFlag", Commercial ? "Y" : "N");
                    //TODO: Figure out DIM Weights
					//writer.WriteElementString("Size", package.IsOversize ? "LARGE" : "REGULAR");
					//writer.WriteElementString("Length", package.RoundedLength.ToString());
					//writer.WriteElementString("Width", package.RoundedWidth.ToString());
					//writer.WriteElementString("Height", package.RoundedHeight.ToString());
					//writer.WriteElementString("Girth", package.CalculatedGirth.ToString());
					i++;
                    writer.WriteEndElement();
				}
				writer.WriteEndElement();
				writer.Flush();
			}

			try
			{
                string url = string.Concat(PRODUCTION_URL, "?API=IntlRateV2&XML=", sb.ToString());
				var webClient = new WebClient();
				string response = webClient.DownloadString(url);
                
				ParseResult(response);
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex);
			}
		}

		public bool IsDomesticUSPSAvailable()
		{
			return Shipment.OriginAddress.IsUnitedStatesAddress() && Shipment.DestinationAddress.IsUnitedStatesAddress();
		}

		private void ParseResult(string response)
		{
			XDocument document = XDocument.Load(new StringReader(response));

			var rates = document.Descendants("Service")
                .GroupBy(item => (string) item.Element("SvcDescription"))
                .Select(g => new
                {
                    Name = g.Key,
                    TotalCharges = g.Sum(x => Decimal.Parse((string) x.Element("Postage")))
                });


		    if (_service == "ALL")
		    {
		        foreach (var r in rates)
		        {
		            string name = Regex.Replace(r.Name, "&lt.*gt;", "");

		            AddRate(name, string.Concat("USPS ", name), r.TotalCharges, DateTime.Now.AddDays(30));

		        }
		    }
		    else
		    {
		        foreach (var r in rates)
		        {
                    string name = Regex.Replace(r.Name, "&lt.*gt;", "");

		            if (_service == name)
                    {
                        AddRate(name, string.Concat("USPS ", name), r.TotalCharges, DateTime.Now.AddDays(30));
		            }
		        }
		    }

		    //check for errors
		    if (document.Elements("Error").Any())
		    {
		        var errors = from item in document.Descendants("Error")
		            select new USPSError()
		            {
		                Description = item.Element("Number").ToString(),
                        Source = item.Element("Source").ToString(),
                        HelpContext = item.Element("HelpContext").ToString(),
                        HelpFile = item.Element("HelpFile").ToString(),
                        Number = item.Element("Number").ToString()
		            };

		        foreach (var err in errors)
		        {
		            AddError(err);
		        }
		    }
		}

		#endregion
	}
}