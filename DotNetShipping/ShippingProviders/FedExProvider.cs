using System;
using System.Collections.Generic;
using System.Configuration;
using DotNetShipping.RateServiceWebReference;

namespace DotNetShipping.ShippingProviders
{
    /// <summary>
    ///     Provides rates from FedEx (Federal Express) excluding SmartPost. Please use <see cref="FedExSmartPostProvider"/> for SmartPost rates.
    /// </summary>
    public class FedExProvider : FedExBaseProvider
    {
        private bool _useAccountRates;

        /// <summary>
        ///     Paramaterless constructor that loads settings from app.config
        /// </summary>
        public FedExProvider()
        {
            var appSettings = ConfigurationManager.AppSettings;
            Init(appSettings["FedExKey"], appSettings["FedExPassword"], appSettings["FedExAccountNumber"], appSettings["FedExMeterNumber"], true, false);
        }

        /// <summary>
        /// </summary>
        /// <param name="key"></param>
        /// <param name="password"></param>
        /// <param name="accountNumber"></param>
        /// <param name="meterNumber"></param>
        public FedExProvider(string key, string password, string accountNumber, string meterNumber)
        {
            Init(key, password, accountNumber, meterNumber, true, false);
        }

        /// <summary>
        /// </summary>
        /// <param name="key"></param>
        /// <param name="password"></param>
        /// <param name="accountNumber"></param>
        /// <param name="meterNumber"></param>
        /// <param name="useProduction"></param>
        /// <param name="useAccountRates">Flag to indicate if you want to use account discounted rates or FedEx listed rates.</param>
        public FedExProvider(string key, string password, string accountNumber, string meterNumber, bool useProduction, bool useAccountRates = false)
        {
            Init(key, password, accountNumber, meterNumber, useProduction, useAccountRates);
        }

        private void Init(string key, string password, string accountNumber, string meterNumber, bool useProduction, bool useAccountRates)
        {
            Name = "FedEx";
            _key = key;
            _password = password;
            _accountNumber = accountNumber;
            _meterNumber = meterNumber;
            _useProduction = useProduction;
            _useAccountRates = useAccountRates;

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
                {"INTERNATIONAL_PRIORITY", "FedEx International Priority"}
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
            request.RequestedShipment.PackagingTypeSpecified = false;

            SetOrigin(request);

            SetDestination(request);

            SetPackageLineItems(request);
            
            request.RequestedShipment.RateRequestTypes = new RateRequestType[1];
            if (_useAccountRates)
                request.RequestedShipment.RateRequestTypes[0] = RateRequestType.ACCOUNT;
            else
                request.RequestedShipment.RateRequestTypes[0] = RateRequestType.LIST;

            request.RequestedShipment.PackageCount = Shipment.PackageCount.ToString();
        }
    }
}
