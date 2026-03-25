namespace GPCS_ExchangeRate.Application.Common.Helpers
{
    public static class ExchangeRateDeltaCalculator
    {
        public static (decimal Delta, decimal DeltaPercentage) Calculate(
            decimal currentRate,
            decimal? previousRate)
        {
            if (previousRate is null or 0)
                return (0m, 0m);

            var delta = currentRate - previousRate.Value;
            var deltaPercentage = (delta / previousRate.Value) * 100m;

            return (
                Math.Round(delta, 4, MidpointRounding.AwayFromZero),
                Math.Round(deltaPercentage, 2, MidpointRounding.AwayFromZero)
            );
        }
    }
}
