using GPCS_ExchangeRate.Domain.Entities;

namespace GPCS_ExchangeRate.Domain.Interfaces
{
    public interface IExchangeRateOutBoxEventsRepository : IGenericRepository<ExchangeRateOutBoxEvents>
    {
        Task<List<ExchangeRateOutBoxEvents>> GetUnprocessedAsync(
            CancellationToken cancellationToken = default);
    }
}
