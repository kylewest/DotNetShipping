using System;

namespace DotNetShipping
{
	/// <summary>
	/// 	Summary description for Rate.
	/// </summary>
	public class Rate : IComparable
	{
		#region Fields

		/// <summary>
		/// 	A description of the rate, as specified by the provider.
		/// </summary>
		public string Description;

		/// <summary>
		/// 	The guaranteed date and time of delivery for this rate.
		/// </summary>
		public DateTime GuaranteedDelivery;

		/// <summary>
		/// 	The name of the rate, as specified by the provider.
		/// </summary>
		public string Name;

		/// <summary>
		/// 	The <see cref = "ShippingProviders.IShippingProvider" /> implementation which provided this rate.
		/// </summary>
		public string Provider;

		/// <summary>
		/// 	The total cost of this rate.
		/// </summary>
		public decimal TotalCharges;

		#endregion

		#region .ctor

		/// <summary>
		/// 	Creates a new instance of the <see cref = "Rate" /> class.
		/// </summary>
		/// <param name = "provider">The name of the provider responsible for this rate.</param>
		/// <param name = "name">The name of the rate.</param>
		/// <param name = "description">A description of the rate.</param>
		/// <param name = "totalCharges">The total cost of this rate.</param>
		/// <param name = "delivery">The guaranteed date and time of delivery for this rate.</param>
		public Rate(string provider, string name, string description, decimal totalCharges, DateTime delivery)
		{
			Provider = provider;
			Name = name;
			Description = description;
			TotalCharges = totalCharges;
			GuaranteedDelivery = delivery;
		}

		#endregion

		#region Methods

		public int CompareTo(object obj)
		{
			var rateB = (Rate) obj;
			return GuaranteedDelivery.CompareTo(rateB.GuaranteedDelivery);
		}

		public override string ToString()
		{
			return Provider + Environment.NewLine + "\t" + Name + Environment.NewLine + "\t" + Description + Environment.NewLine +
			       "\t" + TotalCharges + Environment.NewLine + "\t" + GuaranteedDelivery;
		}

		#endregion
	}
}