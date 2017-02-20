using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace DotNetShipping.ShippingProviders
{
    /// <summary>
    /// </summary>
    public class USPSProvider : AbstractShippingProvider
    {
        private const string PRODUCTION_URL = "http://production.shippingapis.com/ShippingAPI.dll";
        private const string REMOVE_FROM_RATE_NAME = "&lt;sup&gt;&amp;reg;&lt;/sup&gt;";

        /// <summary>
        /// If set to ALL, special service types will not be returned. This is a limitation of the USPS API.
        /// </summary>
        private readonly string _service;

        private readonly string _shipDate;
        private readonly string _userId;

        /// <summary>
        /// Service codes. {0} is a placeholder for 1-Day, 2-Day, 3-Day, Military, DPO or a space
        /// </summary>
        private readonly Dictionary<string, string> _serviceCodes = new Dictionary<string, string>
        {
            {"First-Class Mail Large Envelope","First-Class Mail Large Envelope"},
            {"First-Class Mail Letter","First-Class Mail Letter"},
            {"First-Class Mail Parcel","First-Class Mail Parcel"},
            {"First-Class Mail Postcards","First-Class Mail Postcards"},
            {"Priority Mail {0}","Priority Mail {0}"},
            {"Priority Mail Express {0} Hold For Pickup","Priority Mail Express {0} Hold For Pickup"},
            {"Priority Mail Express {0}","Priority Mail Express {0}"},
            {"Standard Post","Standard Post"},
            {"Media Mail Parcel","Media Mail Parcel"},
            {"Library Mail Parcel","Library Mail Parcel"},
            {"Priority Mail Express {0} Flat Rate Envelope","Priority Mail Express {0} Flat Rate Envelope"},
            {"First-Class Mail Large Postcards","First-Class Mail Large Postcards"},
            {"Priority Mail {0} Flat Rate Envelope","Priority Mail {0} Flat Rate Envelope"},
            {"Priority Mail {0} Medium Flat Rate Box","Priority Mail {0} Medium Flat Rate Box"},
            {"Priority Mail {0} Large Flat Rate Box","Priority Mail {0} Large Flat Rate Box"},
            {"Priority Mail Express {0} Sunday/Holiday Delivery","Priority Mail Express {0} Sunday/Holiday Delivery"},
            {"Priority Mail Express {0} Sunday/Holiday Delivery Flat Rate Envelope","Priority Mail Express {0} Sunday/Holiday Delivery Flat Rate Envelope"},
            {"Priority Mail Express {0} Flat Rate Envelope Hold For Pickup","Priority Mail Express {0} Flat Rate Envelope Hold For Pickup"},
            {"Priority Mail {0} Small Flat Rate Box","Priority Mail {0} Small Flat Rate Box"},
            {"Priority Mail {0} Padded Flat Rate Envelope","Priority Mail {0} Padded Flat Rate Envelope"},
            {"Priority Mail Express {0} Legal Flat Rate Envelope","Priority Mail Express {0} Legal Flat Rate Envelope"},
            {"Priority Mail Express {0} Legal Flat Rate Envelope Hold For Pickup","Priority Mail Express {0} Legal Flat Rate Envelope Hold For Pickup"},
            {"Priority Mail Express {0} Sunday/Holiday Delivery Legal Flat Rate Envelope","Priority Mail Express {0} Sunday/Holiday Delivery Legal Flat Rate Envelope"},
            {"Priority Mail {0} Hold For Pickup","Priority Mail {0} Hold For Pickup"},
            {"Priority Mail {0} Large Flat Rate Box Hold For Pickup","Priority Mail {0} Large Flat Rate Box Hold For Pickup"},
            {"Priority Mail {0} Medium Flat Rate Box Hold For Pickup","Priority Mail {0} Medium Flat Rate Box Hold For Pickup"},
            {"Priority Mail {0} Small Flat Rate Box Hold For Pickup","Priority Mail {0} Small Flat Rate Box Hold For Pickup"},
            {"Priority Mail {0} Flat Rate Envelope Hold For Pickup","Priority Mail {0} Flat Rate Envelope Hold For Pickup"},
            {"Priority Mail {0} Gift Card Flat Rate Envelope","Priority Mail {0} Gift Card Flat Rate Envelope"},
            {"Priority Mail {0} Gift Card Flat Rate Envelope Hold For Pickup","Priority Mail {0} Gift Card Flat Rate Envelope Hold For Pickup"},
            {"Priority Mail {0} Window Flat Rate Envelope","Priority Mail {0} Window Flat Rate Envelope"},
            {"Priority Mail {0} Window Flat Rate Envelope Hold For Pickup","Priority Mail {0} Window Flat Rate Envelope Hold For Pickup"},
            {"Priority Mail {0} Small Flat Rate Envelope","Priority Mail {0} Small Flat Rate Envelope"},
            {"Priority Mail {0} Small Flat Rate Envelope Hold For Pickup","Priority Mail {0} Small Flat Rate Envelope Hold For Pickup"},
            {"Priority Mail {0} Legal Flat Rate Envelope","Priority Mail {0} Legal Flat Rate Envelope"},
            {"Priority Mail {0} Legal Flat Rate Envelope Hold For Pickup","Priority Mail {0} Legal Flat Rate Envelope Hold For Pickup"},
            {"Priority Mail {0} Padded Flat Rate Envelope Hold For Pickup","Priority Mail {0} Padded Flat Rate Envelope Hold For Pickup"},
            {"Priority Mail {0} Regional Rate Box A","Priority Mail {0} Regional Rate Box A"},
            {"Priority Mail {0} Regional Rate Box A Hold For Pickup","Priority Mail {0} Regional Rate Box A Hold For Pickup"},
            {"Priority Mail {0} Regional Rate Box B","Priority Mail {0} Regional Rate Box B"},
            {"Priority Mail {0} Regional Rate Box B Hold For Pickup","Priority Mail {0} Regional Rate Box B Hold For Pickup"},
            {"First-Class Package Service Hold For Pickup","First-Class Package Service Hold For Pickup"},
            {"Priority Mail Express {0} Flat Rate Boxes","Priority Mail Express {0} Flat Rate Boxes"},
            {"Priority Mail Express {0} Flat Rate Boxes Hold For Pickup","Priority Mail Express {0} Flat Rate Boxes Hold For Pickup"},
            {"Priority Mail Express {0} Sunday/Holiday Delivery Flat Rate Boxes","Priority Mail Express {0} Sunday/Holiday Delivery Flat Rate Boxes"},
            {"Priority Mail {0} Regional Rate Box C","Priority Mail {0} Regional Rate Box C"},
            {"Priority Mail {0} Regional Rate Box C Hold For Pickup","Priority Mail {0} Regional Rate Box C Hold For Pickup"},
            {"First-Class Package Service","First-Class Package Service"},
            {"Priority Mail Express {0} Padded Flat Rate Envelope","Priority Mail Express {0} Padded Flat Rate Envelope"},
            {"Priority Mail Express {0} Padded Flat Rate Envelope Hold For Pickup","Priority Mail Express {0} Padded Flat Rate Envelope Hold For Pickup"},
            {"Priority Mail Express {0} Sunday/Holiday Delivery Padded Flat Rate Envelope","Priority Mail Express {0} Sunday/Holiday Delivery Padded Flat Rate Envelope"}
        };

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

        public USPSProvider(string userId, string service, string shipDate)
        {
            Name = "USPS";
            _userId = userId;
            _service = service;
            _shipDate = shipDate;
        }

        /// <summary>
        /// Returns the supported service codes
        /// </summary>
        /// <returns></returns>
        public IDictionary<string, string> GetServiceCodes()
        {
            if (_serviceCodes != null && _serviceCodes.Count > 0)
            {
                var serviceCodes = new Dictionary<string, string>();
                var variableValues = new List<String>() {"1-Day", "2-Day", "3-Day", "Military", "DPO"};

                foreach (var variableValue in variableValues)
                {
                    foreach (var serviceCodeKey in _serviceCodes.Keys)
                    {
                        var serviceCode = _serviceCodes[serviceCodeKey];
                        var swappedServiceCodeKey = serviceCodeKey.Replace("{0}", variableValue);
                        var swappedServiceCode = serviceCode.Replace("{0}", variableValue);
                        
                        if (!serviceCodes.ContainsKey(swappedServiceCode))
                            serviceCodes.Add(swappedServiceCodeKey, swappedServiceCode);
                    }
                }

                return serviceCodes;
            }

            return null;
        }

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
            var signatureOnDeliveryRequired = false;

            var settings = new XmlWriterSettings();

            settings.Indent = false;
            settings.OmitXmlDeclaration = true;
            settings.NewLineHandling = NewLineHandling.None;

            using (var writer = XmlWriter.Create(sb, settings))
            {
                writer.WriteStartElement("RateV4Request");
                writer.WriteAttributeString("USERID", _userId);
                if (!baseRatesOnly)
                {
                    writer.WriteElementString("Revision", "2");
                }
                var i = 0;
                foreach (var package in Shipment.Packages)
                {
                    string size;
                    var container = package.Container;
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
                    if (!string.IsNullOrWhiteSpace(_shipDate))
                    {
                        writer.WriteElementString("ShipDate", _shipDate);
                    }

                    if (package.SignatureRequiredOnDelivery)
                        signatureOnDeliveryRequired = true;

                    writer.WriteEndElement();
                    i++;
                }
                writer.WriteEndElement();
                writer.Flush();
            }

            try
            {
                var url = string.Concat(PRODUCTION_URL, "?API=RateV4&XML=", sb.ToString());
                var webClient = new WebClient();
                var response = webClient.DownloadString(url);
                var specialServiceCodes = new List<String>();

                if (signatureOnDeliveryRequired)
                    specialServiceCodes.Add("119");                                 // 119 represents Adult Signature Required

                ParseResult(response, specialServiceCodes);
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

        private void ParseResult(string response, IList<String> includeSpecialServiceCodes = null)
        {
            var document = XElement.Parse(response, LoadOptions.None);

            var rates = from item in document.Descendants("Postage")
                group item by (string) item.Element("MailService")
                into g
                select new {Name = g.Key,
                            TotalCharges = g.Sum(x => Decimal.Parse((string) x.Element("Rate"))),
                            DeliveryDate = g.Select(x => (string) x.Element("CommitmentDate")).FirstOrDefault(),
                            SpecialServices = g.Select(x => x.Element("SpecialServices")).FirstOrDefault() };

            foreach (var r in rates)
            {
                //string name = r.Name.Replace(REMOVE_FROM_RATE_NAME, string.Empty);
                var name = Regex.Replace(r.Name, "&lt.*&gt;", "");
                var additionalCharges = 0.0m;

                if (includeSpecialServiceCodes != null && includeSpecialServiceCodes.Count > 0)
                {
                    var specialServices = r.SpecialServices.XPathSelectElements("SpecialService").ToList();
                    if (specialServices.Count > 0)
                    {
                        foreach (var specialService in specialServices)
                        {
                            var serviceId = (string)specialService.Element("ServiceID");
                            var price = Decimal.Parse((string) specialService.Element("Price"));

                            if (includeSpecialServiceCodes.Contains(serviceId.ToString()))
                                additionalCharges += price;
                        }
                    }
                }

                if (r.DeliveryDate != null)
                {
                    AddRate(name, string.Concat("USPS ", name), r.TotalCharges + additionalCharges, DateTime.Parse(r.DeliveryDate));
                }
                else
                {
                    AddRate(name, string.Concat("USPS ", name), r.TotalCharges + additionalCharges, DateTime.Now.AddDays(30));
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
