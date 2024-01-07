using Centurion.SeedWork.Web.Foundation.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Centurion.TaskManager.Web.Services;

public class NameUserIdProvider : IUserIdProvider
{
  public string? GetUserId(HubConnectionContext connection)
  {
    return connection.User?.GetUserId();
  }
}