using GPCS_ExchangeRate.Application.Features.ExchangeRates.Dto;
using MediatR;

namespace GPCS_ExchangeRate.Application.Features.ExchangeRates.Commands.CreateExchangeRate;

public class CreateExchangeRateCommand(string? userName) : IRequest<ExchangeRateHeaderDetailDto>
{
    // System will automatically resolve the user name from the current user context and pass it to the command handler.
    public string? UserName { get; set; } = userName;

    /// <summary>Billing period in "yyyyMM" format, e.g. "202603".</summary>
    public string Period { get; set; } = string.Empty;

    public List<CreateExchangeRateItemDto> Items { get; set; } = [];

    // Document properties (will be passed to DocumentService for document creation)
    public string? Title { get; set; }
    public DateTime? EffectiveDate { get; set; }
    public bool IsUrgent { get; set; }
    public string Remarks { get; set; } = string.Empty;
    public string? Comment { get; set; }
}

