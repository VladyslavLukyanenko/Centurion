using System.Security.Claims;
using Grpc.Core;

namespace Centurion.SeedWork.Web;

public static class ServiceCallContextExtensions
{
  public static string? GetUserId(this ClaimsPrincipal self) => self.FindFirst("id")?.Value;
  public static string GetUserId(this ServerCallContext self) => self.GetHttpContext().User.GetUserId()!;
}