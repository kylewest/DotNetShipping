using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace DotNetShipping
{
	/// <summary>
	/// 	Summary description for Shipment.
	/// </summary>
	public class Shipment
	{
		#region Fields

		public readonly Address DestinationAddress;
		public readonly Address OriginAddress;
		public ReadOnlyCollection<Package> Packages;
		public ICollection<IRateAdjuster> RateAdjusters;
		private readonly List<Rate> _rates;

		#endregion

		#region .ctor

		public Shipment(Address originAddress, Address destinationAddress, List<Package> packages)
		{
			OriginAddress = originAddress;
			DestinationAddress = destinationAddress;
			Packages = packages.AsReadOnly();
			_rates = new List<Rate>();
		}
		#endregion

		#region Properties

		public int PackageCount
		{
			get { return Packages.Count; }
		}
		public ReadOnlyCollection<Rate> Rates
		{
			get { return _rates.AsReadOnly(); }
		}

		public decimal TotalPackageWeight
		{
			get { return Packages.Sum(x => x.Weight); }
		}

		public List<Rate> rates
		{
			get { return _rates; }
		}

		#endregion
	}
}