using GPCS_ExchangeRate.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GPCS_ExchangeRate.Infrastructure.Data.Configurations;

public class ExchangeRateDetailConfiguration : IEntityTypeConfiguration<ExchangeRateDetail>
{
    public void Configure(EntityTypeBuilder<ExchangeRateDetail> builder)
    {
        builder.HasKey(d => d.Id);

        builder.Property(d => d.CurrencyCode)
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(d => d.Rate)
            .HasColumnType("decimal(18,6)");

        builder.Property(d => d.Rate2Digit)
            .HasColumnType("decimal(18,2)");

        builder.Property(d => d.Rate4Digit)
            .HasColumnType("decimal(18,4)");

        builder.Property(d => d.CreatedBy)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(d => d.UpdatedBy)
            .HasMaxLength(100);
    }
}
