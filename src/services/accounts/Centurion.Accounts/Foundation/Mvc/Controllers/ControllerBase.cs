using Microsoft.AspNetCore.Mvc;
using Centurion.Accounts.Foundation.Model;

namespace Centurion.Accounts.Foundation.Mvc.Controllers;

[ApiV1HttpRoute("[controller]")]
[ApiController]
public abstract class ControllerBase
  : Microsoft.AspNetCore.Mvc.ControllerBase
{
  public override OkObjectResult Ok(object value)
  {
    return base.Ok(value.ToApiContract());
  }

  public override ObjectResult StatusCode(int statusCode, object value)
  {
    return base.StatusCode(statusCode, value.ToApiContract());
  }

  public override UnauthorizedObjectResult Unauthorized(object value)
  {
    return base.Unauthorized(value.ToApiContract());
  }

  public override NotFoundObjectResult NotFound(object value)
  {
    return base.NotFound(value.ToApiContract());
  }

  public override BadRequestObjectResult BadRequest(object error)
  {
    return base.BadRequest(error.ToApiContract());
  }

  public override UnprocessableEntityObjectResult UnprocessableEntity(object error)
  {
    return base.UnprocessableEntity(error.ToApiContract());
  }

  public override ConflictObjectResult Conflict(object error)
  {
    return base.Conflict(error.ToApiContract());
  }

  public override CreatedResult Created(string uri, object value)
  {
    return base.Created(uri, value.ToApiContract());
  }

  public override CreatedResult Created(Uri uri, object value)
  {
    return base.Created(uri, value.ToApiContract());
  }

  public override CreatedAtActionResult CreatedAtAction(string actionName, string controllerName,
    object routeValues, object value)
  {
    return base.CreatedAtAction(actionName, controllerName, routeValues, value.ToApiContract());
  }

  public override CreatedAtRouteResult CreatedAtRoute(string routeName, object routeValues, object value)
  {
    return base.CreatedAtRoute(routeName, routeValues, value.ToApiContract());
  }

  public override AcceptedResult Accepted(object value)
  {
    return base.Accepted(value.ToApiContract());
  }

  public override AcceptedResult Accepted(string uri, object value)
  {
    return base.Accepted(uri, value.ToApiContract());
  }

  public override AcceptedResult Accepted(Uri uri, object value)
  {
    return base.Accepted(uri, value.ToApiContract());
  }

  public override AcceptedAtActionResult AcceptedAtAction(string actionName, string controllerName,
    object routeValues, object value)
  {
    return base.AcceptedAtAction(actionName, controllerName, routeValues, value.ToApiContract());
  }

  public override AcceptedAtRouteResult AcceptedAtRoute(string routeName, object routeValues, object value)
  {
    return base.AcceptedAtRoute(routeName, routeValues, value.ToApiContract());
  }
}