using Centurion.SeedWork.Web.Foundation.Services;
using LightInject;

namespace Centurion.SeedWork.Web.Composition;

public class WebSeedWorkModule : ICompositionRoot
{
  public void Compose(IServiceRegistry serviceRegistry)
  {
    serviceRegistry.RegisterScoped<IIdentityProvider, ClaimsPrincipalBasedIdentityProvider>();
  }
}