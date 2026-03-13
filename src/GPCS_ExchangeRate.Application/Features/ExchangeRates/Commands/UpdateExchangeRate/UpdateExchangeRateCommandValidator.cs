using FluentValidation;
using GPCS_ExchangeRate.Application.Features.ExchangeRates.Commands.CreateExchangeRate;
using GPCS_ExchangeRate.Domain.Interfaces;

namespace GPCS_ExchangeRate.Application.Features.ExchangeRates.Commands.UpdateExchangeRate
{
    public class UpdateExchangeRateCommandValidator : AbstractValidator<UpdateExchangeRateCommand>
    {
        public UpdateExchangeRateCommandValidator(IDateTimeService dateTimeService)
        {
            RuleFor(x => x.Id)
                .GreaterThan(0)
                .WithMessage("Id must be greater than 0.");

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
                .NotEmpty();

            RuleForEach(x => x.Items)
                .SetValidator(new CreateExchangeRateItemDtoValidator());
        }
    }
}
