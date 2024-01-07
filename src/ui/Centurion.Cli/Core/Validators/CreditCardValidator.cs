using Centurion.Cli.Core.Domain.Profiles;
using FluentValidation;

namespace Centurion.Cli.Core.Validators;

public class CreditCardValidator : AbstractValidator<BillingModel>
{
  public CreditCardValidator()
  {
    RuleFor(_ => _.Cvv).NotEmpty().Matches(@"\d{3,4}");
    RuleFor(_ => _.ExpirationMonth).InclusiveBetween(1, 12);
    RuleFor(_ => _.ExpirationYear).GreaterThanOrEqualTo(DateTime.Now.Year);
    RuleFor(_ => _.CardNumber).CreditCard().NotEmpty();
  }
}