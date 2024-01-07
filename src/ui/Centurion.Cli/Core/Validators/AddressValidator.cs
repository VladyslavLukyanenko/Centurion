using Centurion.Cli.Core.Domain.Profiles;
using FluentValidation;

namespace Centurion.Cli.Core.Validators;

public class AddressValidator : AbstractValidator<AddressModel>
{
  public AddressValidator()
  {
    RuleFor(_ => _.City).NotEmpty();
    RuleFor(_ => _.Line1).NotEmpty();
    RuleFor(_ => _.CountryId).NotEmpty();
    RuleFor(_ => _.ZipCode).NotEmpty();
  }
}