using GPCS_ExchangeRate.Application.Features.ExchangeRates.Commands.Common;

namespace GPCS_ExchangeRate.Application.Features.ExchangeRates.Commands.CancelExchangeRate;

public class CancelExchangeRateCommand : IDocumentActionCommand
{
    public int Id { get; set; }
    public string Comment { get; set; } = null!;
}
