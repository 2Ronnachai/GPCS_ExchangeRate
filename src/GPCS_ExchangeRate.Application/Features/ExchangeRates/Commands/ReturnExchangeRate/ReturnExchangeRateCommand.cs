using GPCS_ExchangeRate.Application.Features.ExchangeRates.Commands.Common;

namespace GPCS_ExchangeRate.Application.Features.ExchangeRates.Commands.ReturnExchangeRate;

public class ReturnExchangeRateCommand : IDocumentActionCommand
{
    public int Id { get; set; }
    public string Comment { get; set; } = null!;
}
