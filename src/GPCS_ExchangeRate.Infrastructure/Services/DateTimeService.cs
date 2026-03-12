using GPCS_ExchangeRate.Domain.Interfaces;
using System.Globalization;

namespace GPCS_ExchangeRate.Infrastructure.Services
{
    public class DateTimeService : IDateTimeService
    {
        private static readonly TimeZoneInfo BangkokTimeZone =
            TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");

        /// <inheritdoc/>
        public DateTime UtcNow => DateTime.UtcNow;

        /// <inheritdoc/>
        /// <remarks>Returns Bangkok time (UTC+7). Used for audit fields so Thai users see local time.</remarks>
        public DateTime Now => TimeZoneInfo.ConvertTime(DateTime.UtcNow, BangkokTimeZone);

        /// <inheritdoc/>
        public CultureInfo CultureInfo => new("th-TH");

        /// <inheritdoc/>
        public DateTime UnixTime => new(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    }
}
