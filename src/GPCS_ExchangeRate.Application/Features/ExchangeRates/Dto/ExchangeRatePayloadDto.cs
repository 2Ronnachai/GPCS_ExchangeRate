namespace GPCS_ExchangeRate.Application.Features.ExchangeRates.Dto
{
    public class ExchangeRatePayloadDto
    {
        public string CurrencyCode { get; set; } = null!;
        public string Period { get; set; } = null!;
        public decimal Rate { get; set; } // 4 Digits
        public decimal Rate2 { get; set; } // Rate
        public string AppUserID { get; set; } = null!;
        public string AppDate { get; set; } = null!; // yyyyMMdd
        public string? UpdUserID { get; set; }
        public string? UpdDate { get; set; } // yyyyMMdd
        public string? UpdPGM { get; set; }
    }
}
