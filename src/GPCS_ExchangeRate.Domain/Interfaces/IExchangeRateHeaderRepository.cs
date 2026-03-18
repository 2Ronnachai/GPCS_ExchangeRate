using GPCS_ExchangeRate.Domain.Entities;

namespace GPCS_ExchangeRate.Domain.Interfaces;

public interface IExchangeRateHeaderRepository : IGenericRepository<ExchangeRateHeader>
{
    Task<ExchangeRateHeader?> GetByPeriodAsync(DateTime period);
    Task<ExchangeRateHeader?> GetWithDetailsAsync(int id);

    Task<List<ExchangeRateHeader>> GetAllWithDetailsAsync();

    Task<IReadOnlyList<ExchangeRateHeader>> GetRecentWithDetailsAsync(int take);
}
