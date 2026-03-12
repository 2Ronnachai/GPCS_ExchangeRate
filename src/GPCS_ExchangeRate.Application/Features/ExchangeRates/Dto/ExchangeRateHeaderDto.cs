namespace GPCS_ExchangeRate.Application.Features.ExchangeRates.Dto;

public class ExchangeRateHeaderDto
{
    public int Id { get; set; }

    /// <summary>Billing period in "yyyyMM" format, e.g. "202603".</summary>
    public string Period { get; set; } = string.Empty;

    public string? DocumentNumber { get; set; }
    public int? DocumentId { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public List<ExchangeRateDetailDto> Details { get; set; } = new();
}
