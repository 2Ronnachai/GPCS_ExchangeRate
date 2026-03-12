using GPCS_ExchangeRate.Application.Features.ExchangeRates.Dto;
using MediatR;

namespace GPCS_ExchangeRate.Application.Features.ExchangeRates.Queries.GetExchangeRateById;

public class GetExchangeRateByIdQuery : IRequest<ExchangeRateHeaderDto?>
{
    public int Id { get; set; }

    public GetExchangeRateByIdQuery(int id) => Id = id;
}
