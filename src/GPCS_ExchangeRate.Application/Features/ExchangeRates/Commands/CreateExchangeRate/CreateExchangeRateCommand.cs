using GPCS_ExchangeRate.Application.Features.ExchangeRates.Dto;
using MediatR;

namespace GPCS_ExchangeRate.Application.Features.ExchangeRates.Commands.CreateExchangeRate;

public class CreateExchangeRateCommand : IRequest<ExchangeRateHeaderDto>
{
    /// <summary>Billing period in "yyyyMM" format, e.g. "202603".</summary>
    public string Period { get; set; } = string.Empty;

    public List<CreateExchangeRateItemDto> Items { get; set; } = new();
}
