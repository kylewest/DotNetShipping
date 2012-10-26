namespace DotNetShipping
{
	/// <summary>
	/// 	Summary description for TrackingActivity.
	/// </summary>
	public class TrackingActivity
	{
		#region Fields

		public readonly string ActivityDate;
		public readonly string ActivityTime;
		public readonly string City;
		public readonly string CountryCode;
		public readonly string State;
		public readonly string StatusDescription;
		public readonly string TrackingNumber;

		#endregion

		#region .ctor

		/// <summary>
		/// 	Creates a new instance of the <see cref = "TrackingActivity" /> class.
		/// </summary>
		/// <param name = "trackingNumber">The tracking number of the package.</param>
		/// <param name = "statusDescription">The description of the activity status code.</param>
		/// <param name = "city">The city from the tracking activity.</param>
		/// <param name = "state">The state from the tracking activity.</param>
		/// <param name = "countryCode">The country code from the tracking activity.</param>
		/// <param name = "activityDate">The recorded date from the tracking activity.</param>
		/// <param name = "activityTime">The recorded time from the tracking activity.</param>
		public TrackingActivity(string trackingNumber, string statusDescription, string city, string state, string countryCode,
		                        string activityDate, string activityTime)
		{
			TrackingNumber = trackingNumber;
			StatusDescription = statusDescription;
			City = city;
			State = state;
			CountryCode = countryCode;
			ActivityDate = activityDate;
			ActivityTime = activityTime;
		}

		#endregion

		#region Methods

		public override string ToString()
		{
			return TrackingNumber + "\n\t" + StatusDescription + "\n\t" + City + "\n\t" + State + "\n\t" + CountryCode + "\n\t" +
			       ActivityDate + "\n\t" + ActivityTime;
		}

		#endregion
	}
}