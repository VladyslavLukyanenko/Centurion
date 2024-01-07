using Microsoft.AspNetCore.SignalR;

namespace Centurion.TaskManager.Web.Hubs;

public class TaskHub : Hub<ITaskHubClient>, ITaskHubServer
{
  public Task Ping(Guid state)
  {
    return Clients.Caller.Pong(state.ToString());
  }
}