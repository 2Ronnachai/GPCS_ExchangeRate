namespace GPCS_ExchangeRate.Application.Features.ExchangeRates.Dto
{
    public class ExchangeRateHeaderDeltaDto : ExchangeRateHeaderDto
    {
        public ICollection<ExchangeRateDetailDeltaDto> Details { get; set; } = [];
    }
}
