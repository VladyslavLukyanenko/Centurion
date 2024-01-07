namespace Centurion.Accounts.App.Services.Email;

public interface IEmailSender
{
  Task SendAsync(EmailMessage message, CancellationToken ct = default);
}