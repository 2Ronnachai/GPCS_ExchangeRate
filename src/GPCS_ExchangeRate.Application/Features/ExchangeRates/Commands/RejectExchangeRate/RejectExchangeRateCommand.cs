using MediatR;

namespace GPCS_ExchangeRate.Application.Features.ExchangeRates.Commands.RejectExchangeRate;

public class RejectExchangeRateCommand : IRequest
{
    public int Id { get; set; }
    public string Comment { get; set; } = null!;
}
