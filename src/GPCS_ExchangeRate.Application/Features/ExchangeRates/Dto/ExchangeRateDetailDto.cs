namespace GPCS_ExchangeRate.Application.Features.ExchangeRates.Dto;

public class ExchangeRateDetailDto
{
    public int Id { get; set; }
    public string CurrencyCode { get; set; } = string.Empty;
    public decimal Rate { get; set; }
    public decimal Rate2Digit { get; set; }
    public decimal Rate4Digit { get; set; }
}
