using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Web.Services.Protocols;

using DotNetShipping.RateServiceWebReference;

namespace DotNetShipping.ShippingProviders
{
    public abstract class FedExBaseProvider : AbstractShippingProvider
    {
        protected string _accountNumber;
        protected string _key;
		protected string _meterNumber;
        protected string _password;
        protected bool _useProduction = true;
        protected Dictionary<string, string> _serviceCodes;

        /// <summary>
        /// FedEx allows insured values for items being shipped except when utilizing SmartPost. This setting will this value to be overwritten.
        /// </summary>
        protected bool _allowInsuredValues = true;

        /// <summary>
        ///     Paramaterless constructor that loads settings from app.config
        /// </summary>
        protected FedExBaseProvider()
        {
            var appSettings = ConfigurationManager.AppSettings;
            Init(appSettings["FedExKey"], appSettings["FedExPassword"], appSettings["FedExAccountNumber"], appSettings["FedExMeterNumber"], true);
        }

        /// <summary>
        /// </summary>
        /// <param name="key"></param>
        /// <param name="password"></param>
        /// <param name="accountNumber"></param>
        /// <param name="meterNumber"></param>
        protected FedExBaseProvider(string key, string password, string accountNumber, string meterNumber)
        {
            Init(key, password, accountNumber, meterNumber, true);
        }

        private void Init(string key, string password, string accountNumber, string meterNumber, bool useProduction)
        {
            Name = "FedEx";

            _key = key;
            _password = password;
            _accountNumber = accountNumber;
            _meterNumber = meterNumber;
            _useProduction = useProduction;
        }

		/// <summary>
        /// Sets service codes.
        /// </summary>
        protected abstract void SetServiceCodes();
		
		/// <summary>
        /// Gets service codes.
        /// </summary>
        /// <returns></returns>
        public IDictionary<string, string> GetServiceCodes()
        {
            if (_serviceCodes != null && _serviceCodes.Count > 0)
            {
                return new Dictionary<string, string>(_serviceCodes);
            }

            return null;
        }

		/// <summary>
        /// Creates the rate request
        /// </summary>
        /// <returns></returns>
        protected RateRequest CreateRateRequest()
        {
            // Build the RateRequest
            var request = new RateRequest();

            request.WebAuthenticationDetail = new WebAuthenticationDetail();
            request.WebAuthenticationDetail.UserCredential = new WebAuthenticationCredential();
            request.WebAuthenticationDetail.UserCredential.Key = _key;
            request.WebAuthenticationDetail.UserCredential.Password = _password;

            request.ClientDetail = new ClientDetail();
            request.ClientDetail.AccountNumber = _accountNumber;
            request.ClientDetail.MeterNumber = _meterNumber;

            request.Version = new VersionId();

            request.ReturnTransitAndCommit = true;
            request.ReturnTransitAndCommitSpecified = true;

            SetShipmentDetails(request);

            return request;
        }

		/// <summary>
        /// Sets shipment details
        /// </summary>
        /// <param name="request"></param>
        protected abstract void SetShipmentDetails(RateRequest request);

		/// <summary>
        /// Gets rates
        /// </summary>
        public override void GetRates()
        {
            var request = CreateRateRequest();
            var service = new RateService(_useProduction);
            try
            {
                // Call the web service passing in a RateRequest and returning a RateReply
                var reply = service.getRates(request);
                //
                if (reply.HighestSeverity == NotificationSeverityType.SUCCESS || reply.HighestSeverity == NotificationSeverityType.NOTE || reply.HighestSeverity == NotificationSeverityType.WARNING)
                {
                    ProcessReply(reply);
                }
                ShowNotifications(reply);
            }
            catch (SoapException e)
            {
                Debug.WriteLine(e.Detail.InnerText);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }
        }

		/// <summary>
        /// Processes the reply
        /// </summary>
        /// <param name="reply"></param>
        protected void ProcessReply(RateReply reply)
        {
            foreach (var rateReplyDetail in reply.RateReplyDetails)
            {
                var netCharge = rateReplyDetail.RatedShipmentDetails.Max(x => x.ShipmentRateDetail.TotalNetCharge.Amount);

                var key = rateReplyDetail.ServiceType.ToString();
                var deliveryDate = rateReplyDetail.DeliveryTimestampSpecified ? rateReplyDetail.DeliveryTimestamp : DateTime.Now.AddDays(30);

                if (_serviceCodes.ContainsKey(key))
                {
                    AddRate(key, _serviceCodes[key], netCharge, deliveryDate);
                }                
            }
        }

		/// <summary>
        /// Sets the destination
        /// </summary>
        /// <param name="request"></param>
        protected void SetDestination(RateRequest request)
        {
            request.RequestedShipment.Recipient = new Party();
            request.RequestedShipment.Recipient.Address = new RateServiceWebReference.Address();
            request.RequestedShipment.Recipient.Address.StreetLines = new string[1] { "" };
            request.RequestedShipment.Recipient.Address.City = "";
            request.RequestedShipment.Recipient.Address.StateOrProvinceCode = "";
            request.RequestedShipment.Recipient.Address.PostalCode = Shipment.DestinationAddress.PostalCode;
            request.RequestedShipment.Recipient.Address.CountryCode = Shipment.DestinationAddress.CountryCode;
            request.RequestedShipment.Recipient.Address.Residential = Shipment.DestinationAddress.IsResidential;
            request.RequestedShipment.Recipient.Address.ResidentialSpecified = Shipment.DestinationAddress.IsResidential;
        }

		/// <summary>
        /// Sets the origin
        /// </summary>
        /// <param name="request"></param>
        protected void SetOrigin(RateRequest request)
        {
            request.RequestedShipment.Shipper = new Party();
            request.RequestedShipment.Shipper.Address = new RateServiceWebReference.Address();
            request.RequestedShipment.Shipper.Address.StreetLines = new string[1] { "" };
            request.RequestedShipment.Shipper.Address.City = "";
            request.RequestedShipment.Shipper.Address.StateOrProvinceCode = "";
            request.RequestedShipment.Shipper.Address.PostalCode = Shipment.OriginAddress.PostalCode;
            request.RequestedShipment.Shipper.Address.CountryCode = Shipment.OriginAddress.CountryCode;
            request.RequestedShipment.Shipper.Address.Residential = Shipment.OriginAddress.IsResidential;
            request.RequestedShipment.Shipper.Address.ResidentialSpecified = Shipment.OriginAddress.IsResidential;
        }

		/// <summary>
        /// Sets package line items
        /// </summary>
        /// <param name="request"></param>
        protected void SetPackageLineItems(RateRequest request)
        {
            request.RequestedShipment.RequestedPackageLineItems = new RequestedPackageLineItem[Shipment.PackageCount];

            var i = 0;
            foreach (var package in Shipment.Packages)
            {
                request.RequestedShipment.RequestedPackageLineItems[i] = new RequestedPackageLineItem();
                request.RequestedShipment.RequestedPackageLineItems[i].SequenceNumber = (i + 1).ToString();
                request.RequestedShipment.RequestedPackageLineItems[i].GroupPackageCount = "1";
                // package weight
                request.RequestedShipment.RequestedPackageLineItems[i].Weight = new Weight();
                request.RequestedShipment.RequestedPackageLineItems[i].Weight.Units = WeightUnits.LB;
                request.RequestedShipment.RequestedPackageLineItems[i].Weight.Value = package.RoundedWeight;
                // package dimensions
                request.RequestedShipment.RequestedPackageLineItems[i].Dimensions = new Dimensions();
                request.RequestedShipment.RequestedPackageLineItems[i].Dimensions.Length = package.RoundedLength.ToString();
                request.RequestedShipment.RequestedPackageLineItems[i].Dimensions.Width = package.RoundedWidth.ToString();
                request.RequestedShipment.RequestedPackageLineItems[i].Dimensions.Height = package.RoundedHeight.ToString();
                request.RequestedShipment.RequestedPackageLineItems[i].Dimensions.Units = LinearUnits.IN;

                if (_allowInsuredValues)
                {
                    // package insured value
                    request.RequestedShipment.RequestedPackageLineItems[i].InsuredValue = new Money();
                    request.RequestedShipment.RequestedPackageLineItems[i].InsuredValue.Amount = package.InsuredValue;
                    request.RequestedShipment.RequestedPackageLineItems[i].InsuredValue.AmountSpecified = true;
                    request.RequestedShipment.RequestedPackageLineItems[i].InsuredValue.Currency = "USD";
                }

                if (package.SignatureRequiredOnDelivery)
                {
                    var signatureOptionDetail = new SignatureOptionDetail { OptionType = SignatureOptionType.DIRECT };
                    var specialServicesRequested = new PackageSpecialServicesRequested() { SignatureOptionDetail = signatureOptionDetail };

                    request.RequestedShipment.RequestedPackageLineItems[i].SpecialServicesRequested = specialServicesRequested;
                }

                i++;
            }
        }

		/// <summary>
        /// Outputs the notifications to the debug console
        /// </summary>
        /// <param name="reply"></param>
        protected static void ShowNotifications(RateReply reply)
        {
            Debug.WriteLine("Notifications");
            for (var i = 0; i < reply.Notifications.Length; i++)
            {
                var notification = reply.Notifications[i];
                Debug.WriteLine("Notification no. {0}", i);
                Debug.WriteLine(" Severity: {0}", notification.Severity);
                Debug.WriteLine(" Code: {0}", notification.Code);
                Debug.WriteLine(" Message: {0}", notification.Message);
                Debug.WriteLine(" Source: {0}", notification.Source);
            }
        }
    }
}
