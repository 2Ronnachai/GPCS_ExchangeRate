using GPCS_ExchangeRate.Domain.Entities;
using GPCS_ExchangeRate.Domain.Interfaces;
using GPCS_ExchangeRate.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GPCS_ExchangeRate.Infrastructure.Repositories;

public class ExchangeRateHeaderRepository(AppDbContext context)
        : GenericRepository<ExchangeRateHeader>(context), IExchangeRateHeaderRepository
{
    public async Task<ExchangeRateHeader?> GetByPeriodAsync(DateTime period)
        => await _dbSet
            .Include(h => h.Details)
            .FirstOrDefaultAsync(h => h.Period.Year == period.Year && h.Period.Month == period.Month);

    public async Task<ExchangeRateHeader?> GetWithDetailsAsync(int id)
        => await _dbSet
            .Include(h => h.Details)
            .FirstOrDefaultAsync(h => h.Id == id);

    public async Task<List<ExchangeRateHeader>> GetAllWithDetailsAsync()
        => await _dbSet
            .Include(h => h.Details)
            .ToListAsync();

    public async Task<IReadOnlyList<ExchangeRateHeader>> GetRecentWithDetailsAsync(int take)
        => await _dbSet
            .Include(h => h.Details)
            .OrderByDescending(h => h.Period)
            .Take(take)
            .ToListAsync();
}
