using MediatR;

namespace GPCS_ExchangeRate.Application.Features.ExchangeRates.Commands.ReturnExchangeRate;

public class ReturnExchangeRateCommand : IRequest
{
    public int Id { get; set; }
    public string Comment { get; set; } = null!;
}
