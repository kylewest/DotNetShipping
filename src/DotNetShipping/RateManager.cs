using System.Collections;
using System.Collections.Generic;
using System.Threading;

using DotNetShipping.ShippingProviders;

namespace DotNetShipping
{
    /// <summary>
    ///     Responsible for coordinating the retrieval of rates from the specified providers for a specified shipment.
    /// </summary>
    public class RateManager
    {
        private readonly IList<IRateAdjuster> _adjusters;
        private readonly ArrayList _providers;

        /// <summary>
        ///     Creates a new RateManager instance.
        /// </summary>
        public RateManager()
        {
            _providers = new ArrayList();
            _adjusters = new List<IRateAdjuster>();
        }

        /// <summary>
        ///     Adds the specified provider to be rated when <see cref="GetRates" /> is called.
        /// </summary>
        /// <param name="provider">A provider-specific implementation of <see cref="ShippingProviders.IShippingProvider" />.</param>
        public void AddProvider(IShippingProvider provider)
        {
            _providers.Add(provider);
        }

        public void AddRateAdjuster(IRateAdjuster adjuster)
        {
            _adjusters.Add(adjuster);
        }

        private Shipment getRates(ref Shipment shipment)
        {
            // create an ArrayList of threads, pre-sized to the number of providers.
            var threads = new ArrayList(_providers.Count);
            // iterate through the providers.
            foreach (AbstractShippingProvider provider in _providers)
            {
                // assign the shipment and ApplyDiscounts value to the provider.
                provider.Shipment = shipment;
                // setting ApplyDiscounts here is overriding provider-specific settings - commenting it out for now
                //if(this._applyDiscounts)
                //	provider.ApplyDiscounts = true;
                // set the ThreadStart method for the thread to the provider's GetRates method.
                var thread = new Thread(provider.GetRates);
                // assign the thread the name of the provider (for debugging purposes).
                thread.Name = provider.Name;
                // start the thread.
                thread.Start();
                // add the thread to our ArrayList.
                threads.Add(thread);
            }
            // loop continuously until all threads have been removed.
            while (threads.Count > 0)
            {
                // loop through the threads (we can't use an iterator since we'll be deleting from the ArrayList).
                for (var x = (threads.Count - 1); x > -1; x--)
                {
                    // check the ThreadState to see if it's Stopped.
                    if (((Thread) threads[x]).ThreadState == ThreadState.Stopped)
                    {
                        // it's stopped, so we'll abort the thread and remove it from the ArrayList.
                        ((Thread) threads[x]).Abort();
                        threads.RemoveAt(x);
                    }
                }
                Thread.Sleep(1);
            }

            // return our Shipment instance.
            return shipment;
        }

        /// <summary>
        ///     Retrieves rates for all of the specified providers using the specified address and package information.
        /// </summary>
        /// <param name="originAddress">An instance of <see cref="Address" /> specifying the origin of the shipment.</param>
        /// <param name="destinationAddress">An instance of <see cref="Address" /> specifying the destination of the shipment.</param>
        /// <param name="package">An instance of <see cref="Package" /> specifying the package to be rated.</param>
        /// <returns>A <see cref="Shipment" /> instance containing all returned rates.</returns>
        public Shipment GetRates(Address originAddress, Address destinationAddress, Package package)
        {
            return GetRates(originAddress, destinationAddress, new List<Package> {package});
        }

        /// <summary>
        ///     Retrieves rates for all of the specified providers using the specified address and packages information.
        /// </summary>
        /// <param name="originAddress">An instance of <see cref="Address" /> specifying the origin of the shipment.</param>
        /// <param name="destinationAddress">An instance of <see cref="Address" /> specifying the destination of the shipment.</param>
        /// <param name="packages">An instance of <see cref="PackageCollection" /> specifying the packages to be rated.</param>
        /// <returns>A <see cref="Shipment" /> instance containing all returned rates.</returns>
        public Shipment GetRates(Address originAddress, Address destinationAddress, List<Package> packages)
        {
            var shipment = new Shipment(originAddress, destinationAddress, packages) {RateAdjusters = _adjusters};
            return getRates(ref shipment);
        }
    }
}
