namespace DotNetShipping
{
    public interface IRateAdjuster
    {
        Rate AdjustRate(Rate rate);
    }
}
