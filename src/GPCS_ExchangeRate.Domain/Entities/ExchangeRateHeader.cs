using GPCS_ExchangeRate.Domain.Common;

namespace GPCS_ExchangeRate.Domain.Entities;

public class ExchangeRateHeader : AuditableEntity
{
    /// <summary>
    /// Stores the first day of the billing month, e.g. 2026-03-01.
    /// UI sends "202603" and the Application layer parses it to DateTime.
    /// </summary>
    public DateTime Period { get; set; }

    /// <summary>Document number assigned by the Document Control API.</summary>
    public string? DocumentNumber { get; set; }

    /// <summary>Document ID assigned by the Document Control API.</summary>
    public int? DocumentId { get; set; }
    public string? DocumentStatus { get; set; }
    public bool IsUrgent { get; set; }
    public DateTime? EffectiveDate { get; set; }
    public string? Remarks { get; set; }
    public DateTime? CompletedAt { get; set; } = null;

    public ICollection<ExchangeRateDetail> Details { get; set; } = [];
}
