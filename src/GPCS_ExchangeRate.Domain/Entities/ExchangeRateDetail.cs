using GPCS_ExchangeRate.Domain.Common;

namespace GPCS_ExchangeRate.Domain.Entities;

public class ExchangeRateDetail : AuditableEntity
{
    public int ExchangeRateHeaderId { get; set; }

    /// <summary>Currency code, e.g. USD, JPY, EUR. Base currency is THB.</summary>
    public string CurrencyCode { get; set; } = string.Empty;

    /// <summary>Exchange rate at full precision as entered by the user.</summary>
    public decimal Rate { get; set; }

    /// <summary>Rate rounded to 2 decimal places. Auto-calculated from Rate.</summary>
    public decimal Rate2Digit { get; set; }

    /// <summary>Rate rounded to 4 decimal places. Auto-calculated from Rate.</summary>
    public decimal Rate4Digit { get; set; }

    public ExchangeRateHeader Header { get; set; } = null!;
}
