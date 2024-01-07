using Centurion.Contracts.Checkout.Integration;

namespace Centurion.TaskManager.Web.Hubs;

public interface ITaskHubClient
{
  Task ProductCheckOutSucceeded(ProductCheckedOut @event);
  Task Pong(string state);
}