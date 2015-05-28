namespace DotNetShipping.ShippingProviders
{
    /// <summary>
    ///     Defines a standard interface for all shipping providers.
    /// </summary>
    public interface IShippingProvider
    {
        /// <summary>
        ///     The name of the provider.
        /// </summary>
        string Name { get; }
        /// <summary>
        ///     The shipment which contains rates from the provider after calling <see cref="GetRates" />.
        /// </summary>
        Shipment Shipment { get; }

        /// <summary>
        ///     Retrieves rates from the provider.
        /// </summary>
        void GetRates();
    }
}
