using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

using DotNetShipping.Helpers.Extensions;
using DotNetShipping.RateServiceWebReference;

namespace DotNetShipping.ShippingProviders
{
    /// <summary>
    ///     Provides SmartPost rates (only) from FedEx (Federal Express).
    /// </summary>
    public class FedExSmartPostProvider : FedExBaseProvider
    {
        /// <summary>
        /// If not using the production Rate API, you can use 5531 as the HubID per FedEx documentation.
        /// </summary>
        private string _hubId;

        /// <summary>
        ///     Paramaterless constructor that loads settings from app.config
        /// </summary>
        public FedExSmartPostProvider()
        {
            var appSettings = ConfigurationManager.AppSettings;
            Init(appSettings["FedExKey"], appSettings["FedExPassword"], appSettings["FedExAccountNumber"], appSettings["FedExMeterNumber"], true, appSettings["FedExHubId"]);
        }

        /// <summary>
        /// </summary>
        /// <param name="key"></param>
        /// <param name="password"></param>
        /// <param name="accountNumber"></param>
        /// <param name="meterNumber"></param>
        /// <param name="hubId">If specified, the FedEx Rate API will only return SmartPost service type rates. Leave empty to get all service types.</param>
        public FedExSmartPostProvider(string key, string password, string accountNumber, string meterNumber, string hubId)
        {
            Init(key, password, accountNumber, meterNumber, true, hubId);
        }

        /// <summary>
        /// </summary>
        /// <param name="key"></param>
        /// <param name="password"></param>
        /// <param name="accountNumber"></param>
        /// <param name="meterNumber"></param>
        /// <param name="hubId">If specified, the FedEx Rate API will only return SmartPost service type rates. Leave empty to get all service types.</param>
        /// <param name="useProduction"></param>
        public FedExSmartPostProvider(string key, string password, string accountNumber, string meterNumber, string hubId, bool useProduction)
        {
            Init(key, password, accountNumber, meterNumber, useProduction, hubId);
        }

        private void Init(string key, string password, string accountNumber, string meterNumber, bool useProduction, string hubId)
        {
            Name = "FedExSmartPost";

            // SmartPost does not allow insured values
            _allowInsuredValues = false;

            _key = key;
            _password = password;
            _accountNumber = accountNumber;
            _meterNumber = meterNumber;
            _useProduction = useProduction;
            _hubId = hubId;

            SetServiceCodes();
        }

        /// <summary>
        /// Sets the service codes.
        /// </summary>
        protected sealed override void SetServiceCodes()
        {
            _serviceCodes = new Dictionary<string, string>
            {
                {"SMART_POST", "FedEx Smart Post"}
            };
        }

        /// <summary>
        /// Sets shipment details
        /// </summary>
        /// <param name="request"></param>
        protected sealed override void SetShipmentDetails(RateRequest request)
        {
            request.RequestedShipment = new RequestedShipment();
            request.RequestedShipment.ShipTimestamp = DateTime.Now; // Shipping date and time
            request.RequestedShipment.ShipTimestampSpecified = true;
            request.RequestedShipment.DropoffType = DropoffType.REGULAR_PICKUP; //Drop off types are BUSINESS_SERVICE_CENTER, DROP_BOX, REGULAR_PICKUP, REQUEST_COURIER, STATION
            request.RequestedShipment.DropoffTypeSpecified = true;
            request.RequestedShipment.PackagingType = PackagingType.YOUR_PACKAGING;
            request.RequestedShipment.PackagingTypeSpecified = true;

            SetOrigin(request);
            SetDestination(request);
            SetPackageLineItems(request);
            SetSmartPostDetails(request);

            request.RequestedShipment.RateRequestTypes = new RateRequestType[1];
            request.RequestedShipment.RateRequestTypes[0] = RateRequestType.LIST;
            request.RequestedShipment.PackageCount = Shipment.PackageCount.ToString();
        }

        /// <summary>
        /// Sets SmartPost details
        /// </summary>
        /// <param name="request"></param>
        private void SetSmartPostDetails(RateRequest request)
        {
            request.RequestedShipment.ServiceType = ServiceType.SMART_POST;
            request.RequestedShipment.ServiceTypeSpecified = true;
            request.RequestedShipment.SmartPostDetail = new SmartPostShipmentDetail { HubId = _hubId, Indicia = SmartPostIndiciaType.PARCEL_SELECT, IndiciaSpecified = true };

            // Handle the various SmartPost Incidia scenarios
            // The ones we should mainly care about are as follows:
            // PRESORTED_STANDARD (less than 1 LB)
            // PARCEL_SELECT (1 LB through 70 LB)

            var weight = request.RequestedShipment.GetTotalWeight();
            if (weight?.Value < 1.0m)
                request.RequestedShipment.SmartPostDetail.Indicia = SmartPostIndiciaType.PRESORTED_STANDARD;
        }
    }
}
