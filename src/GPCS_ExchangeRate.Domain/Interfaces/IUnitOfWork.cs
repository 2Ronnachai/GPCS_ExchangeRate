namespace GPCS_ExchangeRate.Domain.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IExchangeRateHeaderRepository ExchangeRateHeaders { get; }
    IExchangeRateDetailRepository ExchangeRateDetails { get; }
    IExchangeRateOutBoxEventsRepository ExchangeRateOutBoxEvents { get; }

    // ── Persistence ────────────────────────────────────────
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    // ── Transaction ────────────────────────────────────────
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);

    IReadOnlyList<T> GetAddedEntities<T>() where T : class;
    IReadOnlyList<T> GetModifiedEntities<T>() where T : class;
    IReadOnlyList<T> GetDeletedEntities<T>() where T : class;
}
