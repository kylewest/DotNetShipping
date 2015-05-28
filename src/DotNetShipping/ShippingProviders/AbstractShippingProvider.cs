using System;
using System.Linq;

namespace DotNetShipping.ShippingProviders
{
    /// <summary>
    ///     A base implementation of the <see cref="IShippingProvider" /> interface.
    ///     All provider-specific classes should inherit from this class.
    /// </summary>
    public abstract class AbstractShippingProvider : IShippingProvider
    {
        public virtual void GetRates()
        {
        }

        public string Name { get; set; }
        public Shipment Shipment { get; set; }

        protected void AddError(USPSError error)
        {
            Shipment.ServerErrors.Add(error);
        }

        protected void AddRate(string providerCode, string name, decimal totalCharges, DateTime delivery)
        {
            AddRate(new Rate(Name, providerCode, name, totalCharges, delivery));
        }

        protected void AddRate(Rate rate)
        {
            if (Shipment.RateAdjusters != null)
            {
                rate = Shipment.RateAdjusters.Aggregate(rate, (current, adjuster) => adjuster.AdjustRate(current));
            }
            Shipment.Rates.Add(rate);
        }
    }
}
