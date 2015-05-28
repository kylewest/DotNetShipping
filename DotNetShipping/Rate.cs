using System;

namespace DotNetShipping
{
    /// <summary>
    ///     Summary Name for Rate.
    /// </summary>
    public class Rate : IComparable
    {
        /// <summary>
        ///     Creates a new instance of the <see cref="Rate" /> class.
        /// </summary>
        /// <param name="provider">The name of the provider responsible for this rate.</param>
        /// <param name="providerCode">The name of the rate.</param>
        /// <param name="name">A Name of the rate.</param>
        /// <param name="totalCharges">The total cost of this rate.</param>
        /// <param name="delivery">The guaranteed date and time of delivery for this rate.</param>
        public Rate(string provider, string providerCode, string name, decimal totalCharges, DateTime delivery)
        {
            Provider = provider;
            ProviderCode = providerCode;
            Name = name;
            TotalCharges = totalCharges;
            GuaranteedDelivery = delivery;
        }

        /// <summary>
        ///     The guaranteed date and time of delivery for this rate.
        /// </summary>
        public DateTime GuaranteedDelivery { get; set; }
        /// <summary>
        ///     A Name of the rate, as specified by the provider.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        ///     The <see cref="ShippingProviders.IShippingProvider" /> implementation which provided this rate.
        /// </summary>
        public string Provider { get; set; }
        /// <summary>
        ///     The ProviderCode of the rate, as specified by the provider.
        /// </summary>
        public string ProviderCode { get; set; }
        /// <summary>
        ///     The total cost of this rate.
        /// </summary>
        public decimal TotalCharges { get; set; }

        public int CompareTo(object obj)
        {
            var rateB = (Rate) obj;
            return GuaranteedDelivery.CompareTo(rateB.GuaranteedDelivery);
        }

        public override string ToString()
        {
            return Provider + Environment.NewLine + "\t" + ProviderCode + Environment.NewLine + "\t" + Name + Environment.NewLine + "\t" + TotalCharges + Environment.NewLine + "\t" + GuaranteedDelivery;
        }
    }
}
