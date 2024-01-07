using Centurion.Accounts.App.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace Centurion.Accounts.Foundation.Services;

public class RazorViewRenderService
  : IViewRenderService
{
  private readonly IRazorViewEngine _razorViewEngine;
  private readonly IServiceProvider _serviceProvider;
  private readonly ITempDataProvider _tempDataProvider;

  public RazorViewRenderService(IRazorViewEngine razorViewEngine, ITempDataProvider tempDataProvider,
    IServiceProvider serviceProvider)
  {
    _razorViewEngine = razorViewEngine;
    _tempDataProvider = tempDataProvider;
    _serviceProvider = serviceProvider;
  }

  public async Task<string> RenderAsync(string viewName, object model)
  {
    var httpCtx = new DefaultHttpContext
    {
      RequestServices = _serviceProvider
    };

    var tempData = new TempDataDictionary(httpCtx, _tempDataProvider);
    var viewDict = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary())
    {
      Model = model
    };

    var actionCtx = new ActionContext(httpCtx, new RouteData(), new ActionDescriptor());
    var view = _razorViewEngine.FindView(actionCtx, viewName, true);

    using (var writer = new StringWriter())
    {
      var viewCtx = new ViewContext(actionCtx, view.View, viewDict, tempData, writer,
        new HtmlHelperOptions());

      await view.View.RenderAsync(viewCtx);

      return writer.ToString();
    }
  }
}