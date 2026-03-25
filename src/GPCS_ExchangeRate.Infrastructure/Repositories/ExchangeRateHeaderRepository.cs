using GPCS_ExchangeRate.Domain.Entities;
using GPCS_ExchangeRate.Domain.Interfaces;
using GPCS_ExchangeRate.Domain.Specifications;
using GPCS_ExchangeRate.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GPCS_ExchangeRate.Infrastructure.Repositories;

public class ExchangeRateHeaderRepository(AppDbContext context)
        : GenericRepository<ExchangeRateHeader>(context), IExchangeRateHeaderRepository
{
    private IQueryable<ExchangeRateHeader> WithDetails()
        => _dbSet.Include(h => h.Details);

    public async Task<ExchangeRateHeader?> GetByPeriodAsync(DateTime period)
        => await WithDetails()
            .Where(ExchangeRateHeaderSpec.IsForPeriod(period))
            .FirstOrDefaultAsync();

    public async Task<ExchangeRateHeader?> GetWithDetailsAsync(int id)
        => await WithDetails()
            .FirstOrDefaultAsync(h => h.Id == id);

    public async Task<List<ExchangeRateHeader>> GetAllWithDetailsAsync()
        => await WithDetails().ToListAsync();

    public async Task<List<ExchangeRateHeader>> GetRecentCompletedWithDetailsAsync(int take)
        => await WithDetails()
            .Where(ExchangeRateHeaderSpec.IsCompleted())
            .OrderByDescending(h => h.Period)
            .Take(take)
            .ToListAsync();

    public async Task<List<ExchangeRateHeader>> GetPendingApprovalWithDetailsAsync()
        => await WithDetails()
            .Where(ExchangeRateHeaderSpec.IsPending())
            .OrderByDescending(h => h.Period)
            .ToListAsync();

    public async Task<ExchangeRateHeader?> GetPreviousCompletedWithDetailsAsync(
        DateTime period,
        CancellationToken cancellationToken = default)
        => await WithDetails()
            .Where(ExchangeRateHeaderSpec.IsForPreviousPeriod(period)
                .And(ExchangeRateHeaderSpec.IsCompleted()))
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<IReadOnlyList<ExchangeRateHeader>> GetPreviousCompletedBatchAsync(
        IEnumerable<DateTime> periods,
        CancellationToken cancellationToken = default)
        => await WithDetails()
            .Where(ExchangeRateHeaderSpec.IsForPeriods(periods)
                .And(ExchangeRateHeaderSpec.IsCompleted()))
            .ToListAsync(cancellationToken);

    public async Task<bool> ExistByPeriodAsync(
        DateTime period,
        CancellationToken cancellationToken = default)
        => await _dbSet
            .Where(ExchangeRateHeaderSpec.IsForPeriod(period)
                .And(ExchangeRateHeaderSpec.IsCompleted()))
            .AnyAsync(cancellationToken);

    public async Task<bool> ExistByPeriodExcludingIdAsync(
        DateTime period,
        int excludeId,
        CancellationToken cancellationToken = default)
        => await _dbSet
            .Where(ExchangeRateHeaderSpec.IsForPeriod(period)
                .And(ExchangeRateHeaderSpec.IsCompleted()))
            .Where(h => h.Id != excludeId)
            .AnyAsync(cancellationToken);
}
