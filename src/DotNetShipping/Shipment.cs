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
		private readonly List<Rate> _rates;
		public ReadOnlyCollection<Package> Packages;
		public ICollection<IRateAdjuster> RateAdjusters;

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

		public ReadOnlyCollection<Rate> Rates
		{
			get { return _rates.AsReadOnly(); }
		}

		public int PackageCount
		{
			get { return Packages.Count; }
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