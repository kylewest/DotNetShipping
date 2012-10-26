namespace DotNetShipping
{
	/// <summary>
	/// 	Summary description for Address.
	/// </summary>
	public class Address
	{
		#region Fields

		public readonly string City;
		public readonly string CountryCode;
		public readonly string Line1;
		public readonly string Line2;
		public readonly string Line3;
		public readonly string PostalCode;
		public readonly string State;

		#endregion

		#region .ctor

		public Address(string city, string state, string postalCode, string countryCode)
			: this(null, null, null, city, state, postalCode, countryCode)
		{
		}

		public Address(string line1, string line2, string line3, string city, string state, string postalCode,
		               string countryCode)
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
	}
}