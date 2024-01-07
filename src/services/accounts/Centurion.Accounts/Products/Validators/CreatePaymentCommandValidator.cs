using Centurion.Accounts.App.Captcha;
using Centurion.Accounts.Core.Products;
using Centurion.Accounts.Core.Products.Services;
using Centurion.Accounts.Foundation.Authorization;
using Centurion.Accounts.Products.Commands;
using FluentValidation;
using FluentValidation.Results;

namespace Centurion.Accounts.Products.Validators;

public class CreatePaymentCommandValidator : AbstractValidator<CreatePaymentCommand>
{
  private readonly IHttpContextAccessor _httpContextAccessor;
  private readonly IPlanRepository _planRepository;
  private readonly ICaptchaService _captchaService;

  public CreatePaymentCommandValidator(IHttpContextAccessor httpContextAccessor, IPlanRepository planRepository/*,
      ICaptchaService captchaService*/)
  {
    _httpContextAccessor = httpContextAccessor;
    _planRepository = planRepository;
    // _captchaService = captchaService;
    // RuleFor(_ => _).CustomAsync(CaptchaValueMustBeValid);
  }

  private async Task CaptchaValueMustBeValid(CreatePaymentCommand cmd,
    ValidationContext<CreatePaymentCommand> context, CancellationToken ct)
  {
    var dashboardId = _httpContextAccessor.HttpContext!.User.GetDashboardId().GetValueOrDefault();
    Plan? plan = await _planRepository.GetByPasswordAsync(dashboardId, cmd.Password, ct);
    if (plan == null || !plan.ProtectPurchasesWithCaptcha)
    {
      return;
    }

    var captcha = cmd.CaptchaResponse;
    var result = await _captchaService.ValidateAsync(captcha);
    if (result.IsFailure)
    {
      var failure = new ValidationFailure(context.PropertyName, result.Error, context.InstanceToValidate)
      {
        ErrorCode = result.Error,
      };

      context.AddFailure(failure);
    }
  }
}