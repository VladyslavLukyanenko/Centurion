using System.Text;

namespace Centurion.Accounts.App.Services.Email;

public class EmailMessage
{
  public EmailMessage(string senderEmail, string recipientEmail, string subject, string content)
    : this(senderEmail, subject, content, recipientEmails: recipientEmail)
  {
  }

  public EmailMessage(string senderEmail, string subject, string content, params string[] recipientEmails)
  {
    Encoding = Encoding.UTF8;
    From = new List<EmailAddress> {new(senderEmail, "Portal WebSportPlan")};
    To = recipientEmails.Select(email => new EmailAddress(email)).ToList();
    Subject = subject;
    Content = content;
  }

  public IList<EmailAddress> From { get; }

  public IList<EmailAddress> To { get; }

  public string Subject { get; set; }

  public string Content { get; set; }

  public Encoding Encoding { get; }
}