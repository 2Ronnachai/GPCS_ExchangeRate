using GPCS_ExchangeRate.Domain.Entities;

namespace GPCS_ExchangeRate.Domain.Interfaces;

public interface IExchangeRateHeaderRepository : IGenericRepository<ExchangeRateHeader>
{
    Task<ExchangeRateHeader?> GetByPeriodAsync(DateTime period);
    Task<ExchangeRateHeader?> GetWithDetailsAsync(int id);

    Task<List<ExchangeRateHeader>> GetAllWithDetailsAsync();

    Task<List<ExchangeRateHeader>> GetRecentCompletedWithDetailsAsync(int take);
    Task<List<ExchangeRateHeader>> GetPendingApprovalWithDetailsAsync();

    Task<ExchangeRateHeader?> GetPreviousCompletedWithDetailsAsync(
        DateTime period,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ExchangeRateHeader>> GetPreviousCompletedBatchAsync(
        IEnumerable<DateTime> periods,
        CancellationToken cancellationToken = default);

    Task<bool> ExistByPeriodAsync(DateTime period, CancellationToken cancellationToken = default);
    Task<bool> ExistByPeriodExcludingIdAsync(DateTime period, int excludeId, CancellationToken cancellationToken = default);
}
