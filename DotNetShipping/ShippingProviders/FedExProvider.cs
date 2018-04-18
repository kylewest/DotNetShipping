using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Web.Services.Protocols;

using DotNetShipping.Helpers.Extensions;
using DotNetShipping.RateServiceWebReference;

namespace DotNetShipping.ShippingProviders
{
    /// <summary>
    ///     Provides rates from FedEx (Federal Express) excluding SmartPost. Please use <see cref="FedExSmartPostProvider"/> for SmartPost rates.
    /// </summary>
    public class FedExProvider : FedExBaseProvider
    {        
        /// <summary>
        ///     Paramaterless constructor that loads settings from app.config
        /// </summary>
        public FedExProvider()
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
        public FedExProvider(string key, string password, string accountNumber, string meterNumber)
        {
            Init(key, password, accountNumber, meterNumber, true);
        }

        /// <summary>
        /// </summary>
        /// <param name="key"></param>
        /// <param name="password"></param>
        /// <param name="accountNumber"></param>
        /// <param name="meterNumber"></param>
        /// <param name="useProduction"></param>
        public FedExProvider(string key, string password, string accountNumber, string meterNumber, bool useProduction)
        {
            Init(key, password, accountNumber, meterNumber, useProduction);
        }

        private void Init(string key, string password, string accountNumber, string meterNumber, bool useProduction)
        {
            Name = "FedEx";
            _key = key;
            _password = password;
            _accountNumber = accountNumber;
            _meterNumber = meterNumber;
            _useProduction = useProduction;

            SetServiceCodes();
        }

        /// <summary>
        /// Sets service codes.
        /// </summary>
        protected sealed override void SetServiceCodes()
        {
            _serviceCodes = new Dictionary<string, string>
            {
                {"PRIORITY_OVERNIGHT", "FedEx Priority Overnight"},
                {"FEDEX_2_DAY", "FedEx 2nd Day"},
                {"FEDEX_2_DAY_AM", "FedEx 2nd Day A.M."},
                {"STANDARD_OVERNIGHT", "FedEx Standard Overnight"},
                {"FIRST_OVERNIGHT", "FedEx First Overnight"},
                {"FEDEX_EXPRESS_SAVER", "FedEx Express Saver"},
                {"FEDEX_GROUND", "FedEx Ground"},
                {"GROUND_HOME_DELIVERY", "FedEx Ground Residential"},
                {"FEDEX_INTERNATIONAL_GROUND", "FedEx International Ground"},
                {"INTERNATIONAL_ECONOMY", "FedEx International Economy"},
                {"INTERNATIONAL_PRIORITY", "FedEx International Priority"},
                {"INTERNATIONAL_FIRST", "FedEx International First" }
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
            
            request.RequestedShipment.RateRequestTypes = new RateRequestType[1];
            request.RequestedShipment.RateRequestTypes[0] = RateRequestType.LIST;
            request.RequestedShipment.PackageCount = Shipment.PackageCount.ToString();
        }
    }
}
