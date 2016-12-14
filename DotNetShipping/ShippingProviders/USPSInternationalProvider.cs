using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;

namespace DotNetShipping.ShippingProviders
{
    /// <summary>
    /// </summary>
    public class USPSInternationalProvider : AbstractShippingProvider
    {
        private const string PRODUCTION_URL = "http://production.shippingapis.com/ShippingAPI.dll";
        private readonly string _service;
        private readonly string _userId;
        private readonly Dictionary<string, string> _serviceCodes = new Dictionary<string, string>
        {
            {"Priority Mail Express International","Priority Mail Express International"},
            {"Priority Mail International","Priority Mail International"},
            {"Global Express Guaranteed (GXG)","Global Express Guaranteed (GXG)"},
            {"Global Express Guaranteed Document","Global Express Guaranteed Document"},
            {"Global Express Guaranteed Non-Document Rectangular","Global Express Guaranteed Non-Document Rectangular"},
            {"Global Express Guaranteed Non-Document Non-Rectangular","Global Express Guaranteed Non-Document Non-Rectangular"},
            {"Priority Mail International Flat Rate Envelope","Priority Mail International Flat Rate Envelope"},
            {"Priority Mail International Medium Flat Rate Box","Priority Mail International Medium Flat Rate Box"},
            {"Priority Mail Express International Flat Rate Envelope","Priority Mail Express International Flat Rate Envelope"},
            {"Priority Mail International Large Flat Rate Box","Priority Mail International Large Flat Rate Box"},
            {"USPS GXG Envelopes","USPS GXG Envelopes"},
            {"First-Class Mail International Letter","First-Class Mail International Letter"},
            {"First-Class Mail International Large Envelope","First-Class Mail International Large Envelope"},
            {"First-Class Package International Service","First-Class Package International Service"},
            {"Priority Mail International Small Flat Rate Box","Priority Mail International Small Flat Rate Box"},
            {"Priority Mail Express International Legal Flat Rate Envelope","Priority Mail Express International Legal Flat Rate Envelope"},
            {"Priority Mail International Gift Card Flat Rate Envelope","Priority Mail International Gift Card Flat Rate Envelope"},
            {"Priority Mail International Window Flat Rate Envelope","Priority Mail International Window Flat Rate Envelope"},
            {"Priority Mail International Small Flat Rate Envelope","Priority Mail International Small Flat Rate Envelope"},
            {"First-Class Mail International Postcard","First-Class Mail International Postcard"},
            {"Priority Mail International Legal Flat Rate Envelope","Priority Mail International Legal Flat Rate Envelope"},
            {"Priority Mail International Padded Flat Rate Envelope","Priority Mail International Padded Flat Rate Envelope"},
            {"Priority Mail International DVD Flat Rate priced box","Priority Mail International DVD Flat Rate priced box"},
            {"Priority Mail International Large Video Flat Rate priced box","Priority Mail International Large Video Flat Rate priced box"},
            {"Priority Mail Express International Flat Rate Boxes","Priority Mail Express International Flat Rate Boxes"},
            {"Priority Mail Express International Padded Flat Rate Envelope","Priority Mail Express International Padded Flat Rate Envelope"}
        };

        public USPSInternationalProvider()
        {
            Name = "USPS";
            _userId = ConfigurationManager.AppSettings["USPSUserId"];
            _service = "ALL";
        }

        /// <summary>
        /// </summary>
        /// <param name="userId"></param>
        public USPSInternationalProvider(string userId)
        {
            Name = "USPS";
            _userId = userId;
            _service = "ALL";
        }

        /// <summary>
        /// </summary>
        /// <param name="userId"></param>
        public USPSInternationalProvider(string userId, string service)
        {
            Name = "USPS";
            _userId = userId;
            _service = service;
        }

        public bool Commercial { get; set; }

        /// <summary>
        /// Returns the supported service codes
        /// </summary>
        /// <returns></returns>
        public IDictionary<string, string> GetServiceCodes()
        {
            if (_serviceCodes != null && _serviceCodes.Count > 0)
            {
                var serviceCodes = new Dictionary<string, string>();

                foreach (var serviceCodeKey in _serviceCodes.Keys)
                {
                    var serviceCode = _serviceCodes[serviceCodeKey];
                    serviceCodes.Add(serviceCodeKey, serviceCode);
                }

                return serviceCodes;
            }

            return null;
        }

        public override void GetRates()
        {
            var sb = new StringBuilder();

            var settings = new XmlWriterSettings();
            settings.Indent = false;
            settings.OmitXmlDeclaration = true;
            settings.NewLineHandling = NewLineHandling.None;

            using (var writer = XmlWriter.Create(sb, settings))
            {
                writer.WriteStartElement("IntlRateV2Request");
                writer.WriteAttributeString("USERID", _userId);

                writer.WriteElementString("Revision", "2");
                var i = 0;
                foreach (var package in Shipment.Packages)
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
                    writer.WriteElementString("Pounds", package.PoundsAndOunces.Pounds.ToString());
                    writer.WriteElementString("Ounces", package.PoundsAndOunces.Ounces.ToString());
                    writer.WriteElementString("MailType", "Package");
                    writer.WriteElementString("ValueOfContents", package.InsuredValue < 0 ? package.InsuredValue.ToString() : "100"); //todo: figure out best way to come up with insured value
                    writer.WriteElementString("Country", Shipment.DestinationAddress.GetCountryName());
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
                var url = string.Concat(PRODUCTION_URL, "?API=IntlRateV2&XML=", sb.ToString());
                var webClient = new WebClient();
                var response = webClient.DownloadString(url);

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
            var document = XDocument.Load(new StringReader(response));

            var rates = document.Descendants("Service").GroupBy(item => (string) item.Element("SvcDescription")).Select(g => new {Name = g.Key, TotalCharges = g.Sum(x => Decimal.Parse((string) x.Element("Postage")))});

            if (_service == "ALL")
            {
                foreach (var r in rates)
                {
                    var name = Regex.Replace(r.Name, "&lt.*gt;", "");

                    AddRate(name, string.Concat("USPS ", name), r.TotalCharges, DateTime.Now.AddDays(30));
                }
            }
            else
            {
                foreach (var r in rates)
                {
                    var name = Regex.Replace(r.Name, "&lt.*gt;", "");

                    if (_service == name)
                    {
                        AddRate(name, string.Concat("USPS ", name), r.TotalCharges, DateTime.Now.AddDays(30));
                    }
                }
            }

            //check for errors
            if (document.Descendants("Error").Any())
            {
                var errors = from item in document.Descendants("Error")
                    select
                        new USPSError
                        {
                            Description = item.Element("Description").ToString(),
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
    }
}
