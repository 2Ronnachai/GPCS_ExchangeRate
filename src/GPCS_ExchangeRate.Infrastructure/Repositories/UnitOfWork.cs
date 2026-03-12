using GPCS_ExchangeRate.Domain.Interfaces;
using GPCS_ExchangeRate.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace GPCS_ExchangeRate.Infrastructure.Repositories;

public class UnitOfWork(AppDbContext context) : IUnitOfWork
{
    private readonly AppDbContext _context = context;
    private IDbContextTransaction? _transaction;

    private IExchangeRateHeaderRepository? _exchangeRateHeaders;
    private IExchangeRateDetailRepository? _exchangeRateDetails;

    public IExchangeRateHeaderRepository ExchangeRateHeaders
        => _exchangeRateHeaders ??= new ExchangeRateHeaderRepository(_context);

    public IExchangeRateDetailRepository ExchangeRateDetails
        => _exchangeRateDetails ??= new ExchangeRateDetailRepository(_context);

    // ── Persistence ───────────────────────────────────────────────────────────
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => _context.SaveChangesAsync(cancellationToken);

    // ── Transaction ───────────────────────────────────────────────────────────
    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null) return;
        _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await _context.SaveChangesAsync(cancellationToken);
            if (_transaction != null)
                await _transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await RollbackTransactionAsync(cancellationToken);
            throw;
        }
        finally
        {
            DisposeTransaction();
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (_transaction != null)
                await _transaction.RollbackAsync(cancellationToken);
        }
        finally
        {
            DisposeTransaction();
        }
    }

    // ── ChangeTracker helpers ─────────────────────────────────────────────────
    /// <inheritdoc/>
    public IReadOnlyList<T> GetAddedEntities<T>() where T : class
        => _context.ChangeTracker
            .Entries<T>()
            .Where(e => e.State == EntityState.Added)
            .Select(e => e.Entity)
            .ToList()
            .AsReadOnly();

    public IReadOnlyList<T> GetModifiedEntities<T>() where T : class
        => _context.ChangeTracker
            .Entries<T>()
            .Where(e => e.State == EntityState.Modified)
            .Select(e => e.Entity)
            .ToList()
            .AsReadOnly();

    public IReadOnlyList<T> GetDeletedEntities<T>() where T : class
        => _context.ChangeTracker
            .Entries<T>()
            .Where(e => e.State == EntityState.Deleted)
            .Select(e => e.Entity)
            .ToList()
            .AsReadOnly();

    private void DisposeTransaction()
    {
        _transaction?.Dispose();
        _transaction = null;
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
        GC.SuppressFinalize(this);
    }
}
