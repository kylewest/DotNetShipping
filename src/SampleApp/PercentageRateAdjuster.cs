namespace DotNetShipping.SampleApp
{
	public class PercentageRateAdjuster : IRateAdjuster
	{
		#region Fields

		private readonly decimal _amount;

		#endregion

		#region .ctor

		public PercentageRateAdjuster(decimal amount)
		{
			_amount = amount;
		}

		#endregion

		#region Methods

		public Rate AdjustRate(Rate rate)
		{
			rate.TotalCharges = rate.TotalCharges * _amount;
			return rate;
		}

		#endregion
	}
}