namespace DotNetShipping
{
	/// <summary>
	/// 	Summary description for Address.
	/// </summary>
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

		#endregion
	}
}