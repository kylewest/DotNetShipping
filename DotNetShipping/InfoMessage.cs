using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetShipping
{
    public class InfoMessage
    {
        /// <summary>
        /// Shipping provider that generated the message
        /// </summary>
        public Enums.ShippingProvider ShippingProvider { get; set; }

        /// <summary>
        /// Message
        /// </summary>
        public String Message { get; set; }

        public InfoMessage(Enums.ShippingProvider shippingProvider, String message)
        {
            ShippingProvider = shippingProvider;
            Message = message;
        }
    }
}
