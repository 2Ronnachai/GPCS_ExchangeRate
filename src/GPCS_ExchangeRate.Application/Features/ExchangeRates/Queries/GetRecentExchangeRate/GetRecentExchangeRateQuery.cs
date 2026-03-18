using GPCS_ExchangeRate.Application.Features.ExchangeRates.Dto;
using MediatR;

namespace GPCS_ExchangeRate.Application.Features.ExchangeRates.Queries.GetRecentExchangeRate
{
    public class GetRecentExchangeRateQuery : IRequest<List<ExchangeRateHeaderDeltaDto>>
    {
    }
}
