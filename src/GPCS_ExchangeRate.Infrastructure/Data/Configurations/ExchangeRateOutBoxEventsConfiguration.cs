using GPCS_ExchangeRate.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GPCS_ExchangeRate.Infrastructure.Data.Configurations
{
    public class ExchangeRateOutBoxEventsConfiguration : IEntityTypeConfiguration<ExchangeRateOutBoxEvents>
    {
        public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<ExchangeRateOutBoxEvents> builder)
        {
            builder.ToTable("ExchangeRateOutBoxEvents");
            builder.HasKey(e => e.Id);
            builder.Property(e => e.EventType)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(e => e.Payload)
                .IsRequired();

            builder.Property(e => e.Status)
                .HasConversion<string>()
                .IsRequired();

            builder.Property(e => e.RetryCount)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(e => e.MaxRetryAttempts)
                .IsRequired()
                .HasDefaultValue(3);

            builder.Property(e => e.ErrorMessage)
                .HasMaxLength(1000);

            builder.HasIndex(e => e.Status);
            builder.HasIndex(e => e.EventType);
            builder.HasIndex(e => new { e.Status, e.CreatedAt });
        }
    }
}
