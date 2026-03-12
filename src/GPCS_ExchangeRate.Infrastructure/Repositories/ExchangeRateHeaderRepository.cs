using GPCS_ExchangeRate.Domain.Entities;
using GPCS_ExchangeRate.Domain.Interfaces;
using GPCS_ExchangeRate.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GPCS_ExchangeRate.Infrastructure.Repositories;

public class ExchangeRateHeaderRepository
    : GenericRepository<ExchangeRateHeader>, IExchangeRateHeaderRepository
{
    public ExchangeRateHeaderRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<ExchangeRateHeader?> GetByPeriodAsync(DateTime period)
        => await _dbSet
            .Include(h => h.Details)
            .FirstOrDefaultAsync(h => h.Period.Year == period.Year && h.Period.Month == period.Month);

    public async Task<ExchangeRateHeader?> GetWithDetailsAsync(int id)
        => await _dbSet
            .Include(h => h.Details)
            .FirstOrDefaultAsync(h => h.Id == id);
}
