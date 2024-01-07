using MimeKit;
using MimeKit.Text;
using Centurion.Accounts.App.Services.Email;

namespace Centurion.Accounts.Infra.Services.Email;

internal static class NotificationsInternalExtensions
{
  public static MimeMessage ToMimeMessage(this EmailMessage self)
  {
    var message = new MimeMessage();
    message.To.AddRange(self.To.Select(m => new MailboxAddress(m.Name, m.Email)));
    message.From.AddRange(self.From.Select(m => new MailboxAddress(m.Name, m.Email)));
    message.Subject = self.Subject;
    message.Body = new TextPart(TextFormat.Html)
    {
      Text = self.Content
    };

    return message;
  }
}