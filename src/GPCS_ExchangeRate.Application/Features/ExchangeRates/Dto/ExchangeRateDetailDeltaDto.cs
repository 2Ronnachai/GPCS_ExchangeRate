namespace GPCS_ExchangeRate.Application.Features.ExchangeRates.Dto
{
    public class ExchangeRateDetailDeltaDto : ExchangeRateDetailDto
    {
        public decimal Delta { get; set; }
        public decimal DeltaPercentage { get; set; }
    }
}
