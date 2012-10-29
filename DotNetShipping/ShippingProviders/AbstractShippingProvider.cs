namespace DotNetShipping.ShippingProviders
{
	/// <summary>
	/// 	A base implementation of the <see cref = "IShippingProvider" /> interface.
	/// 	All provider-specific classes should inherit from this class.
	/// </summary>
	public abstract class AbstractShippingProvider : IShippingProvider
	{
		#region Fields

		private bool _applyDiscounts = RateManager.DEFAULT_APPLY_DISCOUNTS;
		
		#endregion

		#region Properties

		public bool ApplyDiscounts
		{
			get { return _applyDiscounts; }
			set { _applyDiscounts = value; }
		}

		public string Name { get; set; }
		public Shipment Shipment { get; set; }

		#endregion

		#region Methods

		public virtual void GetRates()
		{
		}

		public virtual Shipment GetTrackingActivity(string trackingNumber)
		{
			return null;
		}

		#endregion
	}
}