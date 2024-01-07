using Centurion.CloudManager.Domain;

namespace Centurion.CloudManager.Web.Services;

public interface INodeLifetimeManager
{
  void Update(IEnumerable<Node> nodes);
}