using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace DotNetShipping
{
    /// <summary>
    ///     Summary description for Shipment.
    /// </summary>
    public class Shipment
    {
        public ReadOnlyCollection<Package> Packages;
        public ICollection<IRateAdjuster> RateAdjusters;
        private readonly List<Rate> _rates;
        private readonly List<USPSError> _serverErrors;
        public readonly Address DestinationAddress;
        public readonly Address OriginAddress;

        public Shipment(Address originAddress, Address destinationAddress, List<Package> packages)
        {
            OriginAddress = originAddress;
            DestinationAddress = destinationAddress;
            Packages = packages.AsReadOnly();
            _rates = new List<Rate>();
            _serverErrors = new List<USPSError>();
        }

        public int PackageCount
        {
            get { return Packages.Count; }
        }
        public List<Rate> Rates
        {
            get { return _rates; }
        }
        public decimal TotalPackageWeight
        {
            get { return Packages.Sum(x => x.Weight); }
        }
        public List<USPSError> ServerErrors
        {
            get { return _serverErrors; }
        }
    }
}
