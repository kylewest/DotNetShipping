using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web.Services.Protocols;

using DotNetShipping.RateServiceWebReference;

namespace DotNetShipping.ShippingProviders
{
	/// <summary>
	/// 	Provides rates from FedEx (Federal Express).
	/// </summary>
	public class FedExProvider : AbstractShippingProvider
	{
		#region Fields

		private readonly string _accountNumber;
		private readonly string _key;
		private readonly string _meterNumber;
		private readonly string _password;

		private readonly Dictionary<string, string> _serviceCodes = new Dictionary<string, string>
		                                                            	{
		                                                            		{"PRIORITY_OVERNIGHT", "FedEx Priority Overnight"},
		                                                            		{"FEDEX_2_DAY", "FedEx 2nd Day"},
		                                                            		{"FEDEX_2_DAY_AM", "FedEx 2nd Day A.M."},
		                                                            		{"STANDARD_OVERNIGHT", "FedEx Standard Overnight"},
		                                                            		{"FIRST_OVERNIGHT", "FedEx First Overnight"},
		                                                            		{"FEDEX_EXPRESS_SAVER", "FedEx Express Saver"},
		                                                            		{"FEDEX_GROUND", "FedEx Ground"},
		                                                            		{"INTERNATIONAL_ECONOMY", "FedEx International Economy"},
		                                                            		{"INTERNATIONAL_PRIORITY", "FedEx International Priority"}
		                                                            	};

		#endregion

		#region .ctor

		///<summary>
		///</summary>
		///<param name = "key"></param>
		///<param name = "password"></param>
		///<param name = "accountNumber"></param>
		///<param name = "meterNumber"></param>
		public FedExProvider(string key, string password, string accountNumber, string meterNumber)
		{
			Name = "FexEx";

			_key = key;
			_password = password;
			_accountNumber = accountNumber;
			_meterNumber = meterNumber;
		}

		#endregion

		#region Methods

		public override void GetRates()
		{
			RateRequest request = CreateRateRequest();
			var service = new RateService();
			try
			{
				// Call the web service passing in a RateRequest and returning a RateReply
				RateReply reply = service.getRates(request);
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

		private static void ShowNotifications(RateReply reply)
		{
			Debug.WriteLine("Notifications");
			for (int i = 0; i < reply.Notifications.Length; i++)
			{
				Notification notification = reply.Notifications[i];
				Debug.WriteLine("Notification no. {0}", i);
				Debug.WriteLine(" Severity: {0}", notification.Severity);
				Debug.WriteLine(" Code: {0}", notification.Code);
				Debug.WriteLine(" Message: {0}", notification.Message);
				Debug.WriteLine(" Source: {0}", notification.Source);
			}
		}

		private RateRequest CreateRateRequest()
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

		private void ProcessReply(RateReply reply)
		{
			foreach (RateReplyDetail rateReplyDetail in reply.RateReplyDetails)
			{
				decimal netCharge = rateReplyDetail.RatedShipmentDetails.Max(x => x.ShipmentRateDetail.TotalNetCharge.Amount);

				string key = rateReplyDetail.ServiceType.ToString();
				DateTime deliveryDate = rateReplyDetail.DeliveryTimestampSpecified ? rateReplyDetail.DeliveryTimestamp : DateTime.Now.AddDays(30);
				var rate = new Rate(Name, key, _serviceCodes[key], netCharge, deliveryDate);
				if (Shipment.RateAdjusters != null)
				{
					rate = Shipment.RateAdjusters.Aggregate(rate, (current, adjuster) => adjuster.AdjustRate(current));
				}
				Shipment.rates.Add(rate);
			}
		}

		private void SetDestination(RateRequest request)
		{
			request.RequestedShipment.Recipient = new Party();
			request.RequestedShipment.Recipient.Address = new RateServiceWebReference.Address();
			request.RequestedShipment.Recipient.Address.StreetLines = new string[1] {""};
			request.RequestedShipment.Recipient.Address.City = "";
			request.RequestedShipment.Recipient.Address.StateOrProvinceCode = "";
			request.RequestedShipment.Recipient.Address.PostalCode = Shipment.DestinationAddress.PostalCode;
			request.RequestedShipment.Recipient.Address.CountryCode = Shipment.DestinationAddress.CountryCode;
		}

		private void SetOrigin(RateRequest request)
		{
			request.RequestedShipment.Shipper = new Party();
			request.RequestedShipment.Shipper.Address = new RateServiceWebReference.Address();
			request.RequestedShipment.Shipper.Address.StreetLines = new string[1] {""};
			request.RequestedShipment.Shipper.Address.City = "";
			request.RequestedShipment.Shipper.Address.StateOrProvinceCode = "";
			request.RequestedShipment.Shipper.Address.PostalCode = Shipment.OriginAddress.PostalCode;
			request.RequestedShipment.Shipper.Address.CountryCode = Shipment.OriginAddress.CountryCode;
		}

		private void SetPackageLineItems(RateRequest request)
		{
			request.RequestedShipment.RequestedPackageLineItems = new RequestedPackageLineItem[Shipment.PackageCount];

			int i = 0;
			foreach (Package package in Shipment.Packages)
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
				// package insured value
				request.RequestedShipment.RequestedPackageLineItems[i].InsuredValue = new Money();
				request.RequestedShipment.RequestedPackageLineItems[i].InsuredValue.Amount = package.InsuredValue;
				request.RequestedShipment.RequestedPackageLineItems[i].InsuredValue.AmountSpecified = true;
				request.RequestedShipment.RequestedPackageLineItems[i].InsuredValue.Currency = "USD";
				i++;
			}
		}

		private void SetShipmentDetails(RateRequest request)
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

		#endregion
	}
}