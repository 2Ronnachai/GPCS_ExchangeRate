using System.Globalization;

namespace GPCS_ExchangeRate.Domain.Interfaces
{
    public interface IDateTimeService
    {
        /// <summary>Current UTC time.</summary>
        DateTime UtcNow { get; }

        /// <summary>Current Bangkok time (UTC+7 / SE Asia Standard Time).</summary>
        DateTime Now { get; }

        /// <summary>Thai culture info (th-TH).</summary>
        CultureInfo CultureInfo { get; }

        /// <summary>Unix epoch (1970-01-01 00:00:00 UTC).</summary>
        DateTime UnixTime { get; }
    }
}
