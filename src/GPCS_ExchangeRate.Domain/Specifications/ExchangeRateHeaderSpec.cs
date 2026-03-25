using GPCS_ExchangeRate.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace GPCS_ExchangeRate.Domain.Specifications
{
    public static class ExchangeRateHeaderSpec
    {
        public static Expression<Func<ExchangeRateHeader, bool>> IsCompleted()
        => h => h.CompletedAt.HasValue
             && h.DocumentStatus != "Rejected"
             && h.DocumentStatus != "Cancelled";

        public static Expression<Func<ExchangeRateHeader, bool>> IsPending()
            => h => h.CompletedAt == null;

        public static Expression<Func<ExchangeRateHeader, bool>> IsForPeriod(DateTime period)
            => h => h.Period.Year == period.Year
                 && h.Period.Month == period.Month;

        public static Expression<Func<ExchangeRateHeader, bool>> IsForPreviousPeriod(DateTime period)
        {
            var prev = period.AddMonths(-1);
            return h => h.Period.Year == prev.Year
                     && h.Period.Month == prev.Month;
        }

        public static Expression<Func<ExchangeRateHeader, bool>> IsForPeriods(
            IEnumerable<DateTime> periods)
        {
            var prevPeriodKeys = periods
                .Select(p => p.AddMonths(-1))
                .Select(p => p.Year * 100 + p.Month)
                .ToHashSet();

            return h => prevPeriodKeys.Contains(h.Period.Year * 100 + h.Period.Month);
        }
    }
}
