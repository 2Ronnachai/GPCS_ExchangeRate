using GPCS_ExchangeRate.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GPCS_ExchangeRate.Infrastructure.Data.Configurations;

public class ExchangeRateHeaderConfiguration : IEntityTypeConfiguration<ExchangeRateHeader>
{
    public void Configure(EntityTypeBuilder<ExchangeRateHeader> builder)
    {
        builder.HasKey(h => h.Id);

        builder.Property(h => h.Period)
            .IsRequired();

        builder.HasIndex(h => h.Period)
            .HasDatabaseName("IX_ExchangeRateHeader_Period");

        builder.Property(h => h.DocumentNumber)
            .HasMaxLength(50);

        builder.Property(h => h.DocumentId);

        builder.Property(h => h.CreatedBy)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(h => h.UpdatedBy)
            .HasMaxLength(100);

        builder.HasMany(h => h.Details)
            .WithOne(d => d.Header)
            .HasForeignKey(d => d.ExchangeRateHeaderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
