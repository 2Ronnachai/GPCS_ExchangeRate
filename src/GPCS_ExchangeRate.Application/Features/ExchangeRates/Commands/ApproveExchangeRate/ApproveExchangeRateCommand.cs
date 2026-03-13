using GPCS_ExchangeRate.Application.Features.ExchangeRates.Commands.Common;

namespace GPCS_ExchangeRate.Application.Features.ExchangeRates.Commands.ApproveExchangeRate;

public class ApproveExchangeRateCommand : IDocumentActionCommand
{
    public int Id { get; set; }
    public string? Comment { get; set; }
}
