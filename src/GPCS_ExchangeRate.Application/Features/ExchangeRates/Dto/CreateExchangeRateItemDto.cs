namespace GPCS_ExchangeRate.Application.Features.ExchangeRates.Dto;

public class CreateExchangeRateItemDto
{
    /// <summary>Currency code, e.g. USD, JPY, EUR.</summary>
    public string CurrencyCode { get; set; } = string.Empty;

    /// <summary>Exchange rate at full precision.</summary>
    public decimal Rate { get; set; }
}
