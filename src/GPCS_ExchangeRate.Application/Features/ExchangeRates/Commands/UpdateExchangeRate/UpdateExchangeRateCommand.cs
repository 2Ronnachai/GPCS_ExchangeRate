using GPCS_ExchangeRate.Application.Features.ExchangeRates.Dto;
using MediatR;

namespace GPCS_ExchangeRate.Application.Features.ExchangeRates.Commands.UpdateExchangeRate
{
    public class UpdateExchangeRateCommand(int id, string? userName) : IRequest<ExchangeRateHeaderDetailDto>
    {
        public int Id { get; set; } = id;

        // Injected from current user
        public string? UserName { get; set; } = userName;

        public string Period { get; set; } = string.Empty;
        public List<CreateExchangeRateItemDto> Items { get; set; } = [];

        // Document fields
        public string? Title { get; set; }
        public DateTime? EffectiveDate { get; set; }
        public bool IsUrgent { get; set; }
        public string? Remarks { get; set; }
        public string? Comment { get; set; }
    }
}
