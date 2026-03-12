using GPCS_ExchangeRate.Domain.Entities;
using GPCS_ExchangeRate.Domain.Interfaces;
using GPCS_ExchangeRate.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GPCS_ExchangeRate.Infrastructure.Repositories;

public class ExchangeRateDetailRepository(AppDbContext context)
        : GenericRepository<ExchangeRateDetail>(context), IExchangeRateDetailRepository
{
    public async Task<IReadOnlyList<ExchangeRateDetail>> GetByHeaderIdAsync(int headerId)
        => await _dbSet
            .Where(d => d.ExchangeRateHeaderId == headerId)
            .ToListAsync();
}
