using MediatR;

namespace GPCS_ExchangeRate.Application.Features.ExchangeRates.Commands.ApproveExchangeRate;

public class ApproveExchangeRateCommand : IRequest
{
    public int Id { get; set; }
    public string? Comment { get; set; }
}
