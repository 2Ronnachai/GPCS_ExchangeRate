namespace GPCS_ExchangeRate.Application.Common.Helpers
{
    public static class PeriodParser
    {
        public static DateTime Parse(string period)
        {
            if (period.Length != 6)
                throw new ArgumentException($"Invalid period format '{period}'.");

            if (!int.TryParse(period[..4], out var year))
                throw new ArgumentException($"Invalid year in period '{period}'.");

            if (!int.TryParse(period[4..], out var month))
                throw new ArgumentException($"Invalid month in period '{period}'.");

            if (year < 2000 || year > 2100)
                throw new ArgumentException($"Year '{year}' is out of range.");

            if (month < 1 || month > 12)
                throw new ArgumentException($"Month '{month}' is invalid.");

            return new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
        }
    }
}
