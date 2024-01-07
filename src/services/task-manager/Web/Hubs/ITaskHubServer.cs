namespace Centurion.TaskManager.Web.Hubs;

public interface ITaskHubServer
{
  Task Ping(Guid state);
}