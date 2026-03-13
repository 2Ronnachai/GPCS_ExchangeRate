using FluentValidation;
using GPCS_ExchangeRate.Application.Features.ExchangeRates.Dto;

namespace GPCS_ExchangeRate.Application.Features.ExchangeRates.Commands.CreateExchangeRate
{
    public class CreateExchangeRateItemDtoValidator : AbstractValidator<CreateExchangeRateItemDto>
    {
        public CreateExchangeRateItemDtoValidator()
        {
            RuleFor(x => x.CurrencyCode)
                .NotEmpty()
                .WithMessage("CurrencyCode is required.")
                .Length(2, 10)
                .WithMessage("CurrencyCode must be between 2 and 10 characters.")
                .Matches(@"^[A-Za-z]+$")
                .WithMessage("CurrencyCode must contain only letters.")
                .Must(code => !code.Equals("THB", StringComparison.OrdinalIgnoreCase))
                .WithMessage("THB is the base currency and cannot be added as an exchange rate.");

            RuleFor(x => x.Rate)
                .GreaterThan(0)
                .WithMessage("Rate must be greater than 0.")
                .LessThanOrEqualTo(999999.999999m)
                .WithMessage("Rate is out of acceptable range.")
                .Must(HaveAtMost6DecimalPlaces)
                .WithMessage("Rate must not have more than 6 decimal places.");
        }

        private static bool HaveAtMost6DecimalPlaces(decimal rate)
        {
            // 10^6 = 1,000,000. If rate * 1,000,000 is an integer, then it has at most 6 decimal places.
            return rate * 1_000_000 == Math.Truncate(rate * 1_000_000);
        }
    }
}
