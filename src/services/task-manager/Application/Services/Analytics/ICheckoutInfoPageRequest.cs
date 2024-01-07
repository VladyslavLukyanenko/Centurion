using Centurion.Accounts.App.Model;
using NodaTime;

namespace Centurion.TaskManager.Application.Services.Analytics;

public interface ICheckoutInfoPageRequest : IFilteredPageRequest
{
  Instant StartAt { get; }
  Instant EndAt { get; }
}