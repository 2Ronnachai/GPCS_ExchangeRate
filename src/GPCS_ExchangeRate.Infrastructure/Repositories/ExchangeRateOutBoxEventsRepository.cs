using GPCS_ExchangeRate.Domain.Entities;
using GPCS_ExchangeRate.Domain.Interfaces;
using GPCS_ExchangeRate.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GPCS_ExchangeRate.Infrastructure.Repositories
{
    public class ExchangeRateOutBoxEventsRepository(AppDbContext context) : 
        GenericRepository<ExchangeRateOutBoxEvents>(context), IExchangeRateOutBoxEventsRepository
    {
        public async Task<List<ExchangeRateOutBoxEvents>> GetUnprocessedAsync(
            CancellationToken cancellationToken = default)
            => await _dbSet
                .Where(e => e.Status == OutBoxStatus.Pending
                         && e.RetryCount < e.MaxRetryAttempts)
                .OrderBy(e => e.CreatedAt)
                .Take(50)
                .ToListAsync(cancellationToken);
    }
}
