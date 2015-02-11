using System;
using System.Configuration;
using System.Diagnostics;
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
    public class USPSProvider : AbstractShippingProvider
    {
        #region Fields

        private const string PRODUCTION_URL = "http://production.shippingapis.com/ShippingAPI.dll";
        private const string REMOVE_FROM_RATE_NAME = "&lt;sup&gt;&amp;reg;&lt;/sup&gt;";
        private readonly string _userId;
        private readonly string _service;

        #endregion

        #region .ctor

        public USPSProvider()
        {
            Name = "USPS";
            _userId = ConfigurationManager.AppSettings["USPSUserId"];
            _service = "ALL";
        }

        /// <summary>
        /// </summary>
        /// <param name="userId"></param>
        public USPSProvider(string userId)
        {
            Name = "USPS";
            _userId = userId;
            _service = "ALL";
        }

        /// <summary>
        /// </summary>
        /// <param name="userId"></param>
        public USPSProvider(string userId, string service)
        {
            Name = "USPS";
            _userId = userId;
            _service = service;
        }

        #endregion

        #region Methods

        public override void GetRates()
        {
            GetRates(false);
        }

        public void GetRates(bool baseRatesOnly)
        {
            // USPS only available for domestic addresses. International is a different API.
            if (!IsDomesticUSPSAvailable())
            {
                return;
            }

            var sb = new StringBuilder();

            var settings = new XmlWriterSettings();
            settings.Indent = false;
            settings.OmitXmlDeclaration = true;
            settings.NewLineHandling = NewLineHandling.None;

            using (XmlWriter writer = XmlWriter.Create(sb, settings))
            {
                writer.WriteStartElement("RateV4Request");
                writer.WriteAttributeString("USERID", _userId);
                if (!baseRatesOnly)
                {
                    writer.WriteElementString("Revision", "2");
                }
                int i = 0;
                foreach (Package package in Shipment.Packages)
                {
                    string size;
                    string container = package.Container;
                    if (IsPackageLarge(package))
                    {
                        size = "LARGE";
                        // Container must be RECTANGULAR or NONRECTANGULAR when SIZE is LARGE
                        if (container == null || container.ToUpperInvariant() != "NONRECTANGULAR")
                        {
                            container = "RECTANGULAR";
                        }
                    }
                    else
                    {
                        size = "REGULAR";
                        if (container == null)
                        {
                            container = string.Empty;
                        }
                    }

                    writer.WriteStartElement("Package");
                    writer.WriteAttributeString("ID", i.ToString());
                    writer.WriteElementString("Service", _service);
                    writer.WriteElementString("ZipOrigination", Shipment.OriginAddress.PostalCode);
                    writer.WriteElementString("ZipDestination", Shipment.DestinationAddress.PostalCode);
                    writer.WriteElementString("Pounds", package.PoundsAndOunces.Pounds.ToString());
                    writer.WriteElementString("Ounces", package.PoundsAndOunces.Ounces.ToString());

                    writer.WriteElementString("Container", container);
                    writer.WriteElementString("Size", size);
                    writer.WriteElementString("Width", package.RoundedWidth.ToString());
                    writer.WriteElementString("Length", package.RoundedLength.ToString());
                    writer.WriteElementString("Height", package.RoundedHeight.ToString());
                    writer.WriteElementString("Girth", package.CalculatedGirth.ToString());
                    writer.WriteElementString("Machinable", IsPackageMachinable(package).ToString());
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

        public bool IsPackageLarge(Package package)
        {
            return (package.IsOversize || package.Width > 12 || package.Length > 12 || package.Height > 12);
        }

        public bool IsPackageMachinable(Package package)
        {
            // Machinable parcels cannot be larger than 27 x 17 x 17 and cannot weight more than 25 lbs.
            if (package.Weight > 25)
            {
                return false;
            }

            return (package.Width <= 27 && package.Height <= 17 && package.Length <= 17) || (package.Width <= 17 && package.Height <= 27 && package.Length <= 17) || (package.Width <= 17 && package.Height <= 17 && package.Length <= 27);
        }

        private void ParseResult(string response)
        {
            XElement document = XElement.Parse(response, LoadOptions.None);

            var rates = from item in document.Descendants("Postage")
                group item by (string) item.Element("MailService")
                into g
                select new {Name = g.Key, TotalCharges = g.Sum(x => Decimal.Parse((string) x.Element("Rate")))};

            foreach (var r in rates)
            {
                //string name = r.Name.Replace(REMOVE_FROM_RATE_NAME, string.Empty);
                string name = Regex.Replace(r.Name, "&lt.*&gt;", "");

                AddRate(name, string.Concat("USPS ", name), r.TotalCharges, DateTime.Now.AddDays(30));
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

        #endregion
    }
}
