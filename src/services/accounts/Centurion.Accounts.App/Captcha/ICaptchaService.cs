using CSharpFunctionalExtensions;

namespace Centurion.Accounts.App.Captcha;

public interface ICaptchaService
{
  ValueTask<Result> ValidateAsync(string captchaResponse);
}