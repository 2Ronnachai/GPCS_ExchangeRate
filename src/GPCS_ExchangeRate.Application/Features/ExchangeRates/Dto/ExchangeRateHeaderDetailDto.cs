namespace GPCS_ExchangeRate.Application.Features.ExchangeRates.Dto;

public class ExchangeRateHeaderDetailDto : ExchangeRateHeaderDto
{
    public virtual List<ExchangeRateDetailDto> Details { get; set; } = [];
}
