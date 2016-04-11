using System;
using System.Collections.Generic;
using System.Globalization;

namespace DotNetShipping
{
    public class Address
    {
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

        public string City { get; set; }
        public string CountryCode { get; set; }
        public string CountryName { get; set; }
        public string Line1 { get; set; }
        public string Line2 { get; set; }
        public string Line3 { get; set; }
        public string PostalCode { get; set; }
        public string State { get; set; }
        public bool IsResidential { get; set; }

        public string GetCountryName()
        {
            if (!string.IsNullOrEmpty(CountryName))
            {
                return CountryName;
            }

            if (string.IsNullOrEmpty(CountryCode))
            {
                return string.Empty;
            }
            try
            { 
                var regionInfo = new RegionInfo(CountryCode);
                return regionInfo.EnglishName;
            }
            catch (ArgumentException e)
            {
                //causes the whole application to crash.
            }

            return string.Empty;
        }

        public bool IsCanadaAddress()
        {
            return !string.IsNullOrEmpty(CountryCode) && string.Equals(CountryCode, "CA", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        ///     Returns true if the CountryCode matches US or one of the US territories.
        /// </summary>
        /// <returns></returns>
        public bool IsUnitedStatesAddress()
        {
            var usAndTerritories = new List<string> {"AS", "GU", "MP", "PR", "UM", "VI", "US"};

            return usAndTerritories.Contains(CountryCode);
        }
    }
}
