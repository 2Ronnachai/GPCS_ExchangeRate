using GPCS_ExchangeRate.Domain.Common;
using GPCS_ExchangeRate.Domain.Common.Interfaces;
using GPCS_ExchangeRate.Domain.Entities;
using GPCS_ExchangeRate.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GPCS_ExchangeRate.Infrastructure.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options,
    ICurrentUserService currentUserService,
    IDateTimeService dateTimeService) : DbContext(options)
{
    private readonly IDateTimeService _dateTimeService = dateTimeService;
    private readonly ICurrentUserService _currentUserService = currentUserService;

    public DbSet<ExchangeRateHeader> ExchangeRateHeaders => Set<ExchangeRateHeader>();
    public DbSet<ExchangeRateDetail> ExchangeRateDetails => Set<ExchangeRateDetail>();

    // For OutBox pattern to store events related to exchange rate changes
    public DbSet<ExchangeRateOutBoxEvents> ExchangeRateOutBoxEvents => Set<ExchangeRateOutBoxEvents>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }

    // ── SaveChanges overrides ─────────────────────────────────────────────────
    public override int SaveChanges()
    {
        ApplyAuditInformation();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyAuditInformation();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void ApplyAuditInformation()
    {
        var currentUser = _currentUserService.GetUserName();
        var now = _dateTimeService.Now; // Bangkok time (UTC+7)

        foreach (var entry in ChangeTracker.Entries<IAuditableEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedBy = currentUser;
                    entry.Entity.CreatedAt = now;
                    // Clear UpdatedBy/UpdatedAt on new entities
                    entry.Entity.UpdatedBy = null;
                    entry.Entity.UpdatedAt = null;
                    break;

                case EntityState.Modified:
                    entry.Entity.UpdatedBy = currentUser;
                    entry.Entity.UpdatedAt = now;
                    // Prevent overwriting creation audit fields
                    entry.Property(nameof(IAuditableEntity.CreatedBy)).IsModified = false;
                    entry.Property(nameof(IAuditableEntity.CreatedAt)).IsModified = false;
                    break;
            }
        }
    }
}
