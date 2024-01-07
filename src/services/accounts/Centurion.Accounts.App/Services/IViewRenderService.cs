namespace Centurion.Accounts.App.Services;

public interface IViewRenderService
{
  Task<string> RenderAsync(string viewName, object model);
}