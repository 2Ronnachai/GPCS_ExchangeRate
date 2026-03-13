using MediatR;

namespace GPCS_ExchangeRate.Application.Features.ExchangeRates.Commands.DeleteExchangeRate;

public class DeleteExchangeRateCommand : IRequest
{
    public int Id { get; set; }
}
