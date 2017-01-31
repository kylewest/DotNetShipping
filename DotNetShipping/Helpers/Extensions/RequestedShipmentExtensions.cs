using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using DotNetShipping.RateServiceWebReference;

namespace DotNetShipping.Helpers.Extensions
{
    /// <summary>
    /// Extension methods for RequestedShipment
    /// </summary>
    public static class RequestedShipmentExtensions
    {
        /// <summary>
        /// Calculates the total weight for all packages in the requested shipment
        /// </summary>
        /// <param name="requestedShipment"></param>
        /// <returns>Calculated weight if RequestedPackageLineItems in RequestedShiment, otherwise null.</returns>
        public static Weight GetTotalWeight(this RequestedShipment requestedShipment)
        {
            if (requestedShipment != null && requestedShipment.RequestedPackageLineItems != null && requestedShipment.RequestedPackageLineItems.Length > 0)
            {
                var weight = new Weight { Units = requestedShipment.RequestedPackageLineItems[0].Weight.Units };

                foreach (var requestedPackageLineItem in requestedShipment.RequestedPackageLineItems)
                {
                    weight.Value += requestedPackageLineItem.Weight.Value;
                }

                return weight;
            }

            return null;
        }
    }
}
