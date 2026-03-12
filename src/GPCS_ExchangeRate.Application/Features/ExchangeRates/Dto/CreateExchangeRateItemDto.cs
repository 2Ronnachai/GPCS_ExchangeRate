namespace GPCS_ExchangeRate.Application.Features.ExchangeRates.Dto;

public class CreateExchangeRateItemDto
{
    /// <summary>รหัสสกุลเงิน เช่น USD, JPY, EUR</summary>
    public string CurrencyCode { get; set; } = string.Empty;

    /// <summary>อัตราแลกเปลี่ยน full precision</summary>
    public decimal Rate { get; set; }
}
