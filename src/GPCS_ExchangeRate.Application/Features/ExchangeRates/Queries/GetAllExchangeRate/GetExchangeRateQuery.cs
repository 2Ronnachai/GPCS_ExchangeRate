using GPCS_ExchangeRate.Application.Features.ExchangeRates.Dto;
using MediatR;

namespace GPCS_ExchangeRate.Application.Features.ExchangeRates.Queries.GetAllExchangeRate
{
    public class GetExchangeRateQuery : IRequest<List<ExchangeRateHeaderDetailDto>>
    {
    }
}
