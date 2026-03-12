using GPCS_ExchangeRate.Application.Features.ExchangeRates.Dto;
using MediatR;

namespace GPCS_ExchangeRate.Application.Features.ExchangeRates.Queries.GetExchangeRatesByPeriod;

public class GetExchangeRatesByPeriodQuery : IRequest<ExchangeRateHeaderDto?>
{
    /// <summary>งวดในรูปแบบ "yyyyMM" เช่น "202603"</summary>
    public string Period { get; set; } = string.Empty;

    public GetExchangeRatesByPeriodQuery(string period) => Period = period;
}
