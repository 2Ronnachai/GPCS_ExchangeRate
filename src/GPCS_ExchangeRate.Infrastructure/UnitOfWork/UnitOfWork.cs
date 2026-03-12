using GPCS_ExchangeRate.Domain.Interfaces;
using GPCS_ExchangeRate.Infrastructure.Data;
using GPCS_ExchangeRate.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore.Storage;

namespace GPCS_ExchangeRate.Infrastructure.UnitOfWork;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;
    private IDbContextTransaction? _transaction;

    private IExchangeRateHeaderRepository? _exchangeRateHeaders;
    private IExchangeRateDetailRepository? _exchangeRateDetails;

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
    }

    public IExchangeRateHeaderRepository ExchangeRateHeaders
        => _exchangeRateHeaders ??= new ExchangeRateHeaderRepository(_context);

    public IExchangeRateDetailRepository ExchangeRateDetails
        => _exchangeRateDetails ??= new ExchangeRateDetailRepository(_context);

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => await _context.SaveChangesAsync(cancellationToken);

    public async Task BeginTransactionAsync()
        => _transaction = await _context.Database.BeginTransactionAsync();

    public async Task CommitAsync()
    {
        if (_transaction is not null)
        {
            await _transaction.CommitAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackAsync()
    {
        if (_transaction is not null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}
