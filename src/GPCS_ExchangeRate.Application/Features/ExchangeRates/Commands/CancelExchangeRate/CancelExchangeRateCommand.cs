using MediatR;

namespace GPCS_ExchangeRate.Application.Features.ExchangeRates.Commands.CancelExchangeRate;

public class CancelExchangeRateCommand : IRequest
{
    public int Id { get; set; }
    public string Comment { get; set; } = null!;
}
