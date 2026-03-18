namespace GPCS_ExchangeRate.Application.Features.ExchangeRates.Dto
{
    public class ExchangeRateHeaderDto
    {
        public int Id { get; set; }

        /// <summary>Billing period in "yyyyMM" format, e.g. "202603".</summary>
        public string Period { get; set; } = string.Empty;

        // Document properties from Document Control API
        public string? DocumentNumber { get; set; }
        public int? DocumentId { get; set; }
        public string DocumentStatus { get; set; } = string.Empty;
        public bool IsUrgent { get; set; }
        public DateTime? EffectiveDate { get; set; }
        public string? Remarks { get; set; }
        public DateTime? CompletedAt { get; set; }

        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
    }
}
