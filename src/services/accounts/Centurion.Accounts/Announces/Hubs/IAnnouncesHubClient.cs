namespace Centurion.Accounts.Announces.Hubs;

public interface IAnnouncesHubClient
{
  Task ReceiveAnnounce(string title, string message);
}