using GPCS_ExchangeRate.Application.Features.ExchangeRates.Dto;
using MediatR;

namespace GPCS_ExchangeRate.Application.Features.ExchangeRates.Queries.GetExchangeRatesByPeriod;

public class GetExchangeRatesByPeriodQuery(string period) : IRequest<ExchangeRateHeaderDto?>
{
    /// <summary>Billing period in "yyyyMM" format, e.g. "202603".</summary>
    public string Period { get; set; } = period;
}
