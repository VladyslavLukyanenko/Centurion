namespace Centurion.Accounts.App.Config;

public record ReCaptchaConfig(string SiteKey, string SecretKey, string VerificationUrl);