using System;
using System.Collections.Generic;

namespace DotNetShipping
{
	public class Address
	{
		#region .ctor

		public Address(string city, string state, string postalCode, string countryCode) : this(null, null, null, city, state, postalCode, countryCode)
		{
		}

		public Address(string line1, string line2, string line3, string city, string state, string postalCode, string countryCode)
		{
			Line1 = line1;
			Line2 = line2;
			Line3 = line3;
			City = city;
			State = state;
			PostalCode = postalCode;
			CountryCode = countryCode;
			IsResidential = false;
		}

		#endregion

		#region Properties

		public string City { get; set; }
		public string CountryCode { get; set; }
		public string Line1 { get; set; }
		public string Line2 { get; set; }
		public string Line3 { get; set; }
		public string PostalCode { get; set; }
		public string State { get; set; }
		public bool IsResidential { get; set; }

		#endregion

		#region Methods

		/// <summary>
		/// Returns true if the CountryCode matches US or one of the US territories.
		/// </summary>
		/// <returns></returns>
		public bool IsUnitedStatesAddress()
		{
		    var usAndTerritories = new List<string> {"AS", "GU", "MP", "PR", "UM", "VI", "US"};

		    return usAndTerritories.Contains(CountryCode);
		    
		}

        public bool IsCanadaAddress()
        {
            return !string.IsNullOrEmpty(CountryCode) && string.Equals(CountryCode, "CA", StringComparison.OrdinalIgnoreCase);
        }

		#endregion
	}
}
