using MediatR;

namespace GPCS_ExchangeRate.Application.Features.ExchangeRates.Commands.SubmitExchangeRate;

public class SubmitExchangeRateCommand : IRequest
{
    public SubmitExchangeRateCommand(string? userName)
    {
        UserName = userName;
    }

    public string? UserName { get; set; }

    public int Id { get; set; }

    // Document properties — required only when DocumentId is null (first-time submit)
    public string? Title { get; set; }
    public DateTime? EffectiveDate { get; set; }
    public bool IsUrgent { get; set; }
    public string Remarks { get; set; } = string.Empty;
    public string? Comment { get; set; }
}