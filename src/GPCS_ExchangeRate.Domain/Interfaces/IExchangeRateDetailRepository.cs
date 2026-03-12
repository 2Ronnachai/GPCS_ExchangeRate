using GPCS_ExchangeRate.Domain.Entities;

namespace GPCS_ExchangeRate.Domain.Interfaces;

public interface IExchangeRateDetailRepository : IGenericRepository<ExchangeRateDetail>
{
    Task<IReadOnlyList<ExchangeRateDetail>> GetByHeaderIdAsync(int headerId);
}
