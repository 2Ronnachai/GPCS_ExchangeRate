namespace GPCS_ExchangeRate.Domain.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IExchangeRateHeaderRepository ExchangeRateHeaders { get; }
    IExchangeRateDetailRepository ExchangeRateDetails { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync();
    Task CommitAsync();
    Task RollbackAsync();
}
