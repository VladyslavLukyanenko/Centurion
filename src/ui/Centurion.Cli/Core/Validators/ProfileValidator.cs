using Centurion.Cli.Core.Domain.Profiles;
using FluentValidation;

namespace Centurion.Cli.Core.Validators;

public class ProfileValidator : AbstractValidator<ProfileModel>
{
  private readonly CreditCardValidator _creditCardValidator;

  public ProfileValidator(AddressValidator addressValidator, CreditCardValidator creditCardValidator)
  {
    _creditCardValidator = creditCardValidator;
    RuleFor(_ => _.Name).NotEmpty();
    RuleFor(_ => _.FirstName).NotEmpty();
    RuleFor(_ => _.LastName).NotEmpty();
    RuleFor(_ => _.PhoneNumber).NotEmpty();
    RuleFor(_ => _.BillingAddress).SetValidator(addressValidator!);
    RuleFor(_ => _.ShippingAddress).Must((p, a) => p.BillingAsShipping || addressValidator.Validate(a).IsValid);
    RuleFor(_ => _.Billing).MustAsync(BeValidOrNull);
  }

  private async Task<bool> BeValidOrNull(ProfileModel profile, BillingModel? billing, ValidationContext<ProfileModel> ctx, CancellationToken ct)
  {
    if (billing is null)
    {
      return true;
    }

    var result = await _creditCardValidator.ValidateAsync(billing, ct);
    foreach (var failure in result.Errors)
    {
      ctx.AddFailure(failure);
    }

    return result.IsValid;
  }
}