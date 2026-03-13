using FluentValidation;
using GPCS_ExchangeRate.Application.Features.ExchangeRates.Dto;
using GPCS_ExchangeRate.Domain.Interfaces;

namespace GPCS_ExchangeRate.Application.Features.ExchangeRates.Commands.CreateExchangeRate
{
    public class CreateExchangeRateCommandValidator : AbstractValidator<CreateExchangeRateCommand>
    {
        public CreateExchangeRateCommandValidator(IDateTimeService dateTimeService)
        {
            RuleFor(x => x.UserName)
                .NotEmpty()
                .WithMessage("UserName is required.");

            RuleFor(x => x.Period)
                .NotEmpty()
                .WithMessage("Period is required.")
                .Length(6)
                .WithMessage("Period must be exactly 6 characters (e.g. 202603).")
                .Matches(@"^\d{6}$")
                .WithMessage("Period must contain only digits (e.g. 202603).");

            RuleFor(x => x.Items)
                .NotNull()
                .WithMessage("Items is required.")
                .NotEmpty()
                .WithMessage("At least one exchange rate item is required.");

            RuleForEach(x => x.Items)
                .SetValidator(new CreateExchangeRateItemDtoValidator());

            RuleFor(x => x.Items)
                .Must(HaveUniqueCurrencyCodes)
                .WithMessage("Duplicate currency codes are not allowed in the same request.")
                .When(x => x.Items is { Count: > 0 });
        }

        private static bool HaveUniqueCurrencyCodes(List<CreateExchangeRateItemDto> items)
        {
            var codes = items
                .Select(i => i.CurrencyCode?.ToUpperInvariant())
                .Where(c => c is not null)
                .ToList();

            return codes.Count == codes.Distinct().Count();
        }
    }
}
