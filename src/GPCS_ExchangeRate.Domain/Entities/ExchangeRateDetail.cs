using GPCS_ExchangeRate.Domain.Common;

namespace GPCS_ExchangeRate.Domain.Entities;

public class ExchangeRateDetail : AuditableEntity
{
    public int ExchangeRateHeaderId { get; set; }

    /// <summary>รหัสสกุลเงิน เช่น USD, JPY, EUR (Base = THB)</summary>
    public string CurrencyCode { get; set; } = string.Empty;

    /// <summary>อัตราแลกเปลี่ยน full precision</summary>
    public decimal Rate { get; set; }

    /// <summary>อัตราแลกเปลี่ยนปัดเป็น 2 ตำแหน่ง</summary>
    public decimal Rate2Digit { get; set; }

    /// <summary>อัตราแลกเปลี่ยนปัดเป็น 4 ตำแหน่ง</summary>
    public decimal Rate4Digit { get; set; }

    public ExchangeRateHeader Header { get; set; } = null!;
}
