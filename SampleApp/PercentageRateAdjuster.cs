namespace DotNetShipping.SampleApp
{
	public class PercentageRateAdjuster : IRateAdjuster
	{
		private readonly decimal _amount;

		public PercentageRateAdjuster(decimal amount)
		{
			_amount = amount;
		}

		#region IRateAdjuster Members

		public Rate AdjustRate(Rate rate)
		{
			rate.TotalCharges = rate.TotalCharges*_amount;
			return rate;
		}

		#endregion
	}
}