using GPCS_ExchangeRate.Domain.Common;

namespace GPCS_ExchangeRate.Domain.Entities;

public class ExchangeRateHeader : AuditableEntity
{
    /// <summary>
    /// งวดของ Exchange Rate เก็บเป็นวันแรกของเดือน เช่น 2026-03-01
    /// UI ส่งมาแบบ "202603" → parse เป็น DateTime ใน Application layer
    /// </summary>
    public DateTime Period { get; set; }

    /// <summary>เลขเอกสารจาก Document Control API</summary>
    public string? DocumentNumber { get; set; }

    /// <summary>รหัสอ้างอิงจาก Document Control API</summary>
    public string? DocumentId { get; set; }

    public ICollection<ExchangeRateDetail> Details { get; set; } = new List<ExchangeRateDetail>();
}
