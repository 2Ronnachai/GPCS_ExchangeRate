using GPCS_ExchangeRate.Application.Features.ExchangeRates.Dto;
using MediatR;

namespace GPCS_ExchangeRate.Application.Features.ExchangeRates.Queries.GetExchangeRateById;

public class GetExchangeRateByIdQuery(int id) : IRequest<ExchangeRateHeaderDto?>
{
    public int Id { get; set; } = id;
}
