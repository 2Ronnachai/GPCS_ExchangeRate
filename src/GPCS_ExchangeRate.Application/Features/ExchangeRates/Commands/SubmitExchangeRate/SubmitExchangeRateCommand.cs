using GPCS_ExchangeRate.Application.Features.ExchangeRates.Dto;
using MediatR;

namespace GPCS_ExchangeRate.Application.Features.ExchangeRates.Commands.SubmitExchangeRate;

public class SubmitExchangeRateCommand : IRequest
{
    public SubmitExchangeRateCommand(string? userName) => UserName = userName;

    public string? UserName { get; set; }

    public int Id { get; set; }

    /// <summary>Billing period in "yyyyMM" format, e.g. "202603".</summary>
    public string Period { get; set; } = string.Empty;

    public List<CreateExchangeRateItemDto> Items { get; set; } = [];

    // Document properties — required only when DocumentId is null (first-time submit)
    public string? Title { get; set; }
    public DateTime? EffectiveDate { get; set; }
    public bool IsUrgent { get; set; }
    public string Remarks { get; set; } = string.Empty;
    public string? Comment { get; set; }
}