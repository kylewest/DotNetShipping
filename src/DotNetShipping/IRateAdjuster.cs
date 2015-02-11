namespace DotNetShipping
{
    public interface IRateAdjuster
    {
        #region Methods

        Rate AdjustRate(Rate rate);

        #endregion
    }
}
