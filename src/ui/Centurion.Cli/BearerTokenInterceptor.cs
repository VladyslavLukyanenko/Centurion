using Centurion.Cli.Core.Services;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Microsoft.Extensions.Logging;

namespace Centurion.Cli;

public class BearerTokenInterceptor : Interceptor
{
  private readonly ILogger<BearerTokenInterceptor> _logger;
  private readonly ITokenProvider _token;

  public BearerTokenInterceptor(ILogger<BearerTokenInterceptor> logger, ITokenProvider token)
  {
    _logger = logger;
    _token = token;
  }

  public override TResponse BlockingUnaryCall<TRequest, TResponse>(TRequest request,
    ClientInterceptorContext<TRequest, TResponse> context,
    BlockingUnaryCallContinuation<TRequest, TResponse> continuation)
  {
    AddAuthCreds(context);
    return base.BlockingUnaryCall(request, context, continuation);
  }

  public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(TRequest request,
    ClientInterceptorContext<TRequest, TResponse> context,
    AsyncUnaryCallContinuation<TRequest, TResponse> continuation)
  {
    AddAuthCreds(context);
    return base.AsyncUnaryCall(request, context, continuation);
  }

  public override AsyncClientStreamingCall<TRequest, TResponse> AsyncClientStreamingCall<TRequest, TResponse>(
    ClientInterceptorContext<TRequest, TResponse> context,
    AsyncClientStreamingCallContinuation<TRequest, TResponse> continuation)
  {
    AddAuthCreds(context);
    return base.AsyncClientStreamingCall(context, continuation);
  }

  public override AsyncServerStreamingCall<TResponse> AsyncServerStreamingCall<TRequest, TResponse>(
    TRequest request,
    ClientInterceptorContext<TRequest, TResponse> context,
    AsyncServerStreamingCallContinuation<TRequest, TResponse> continuation)
  {
    AddAuthCreds(context);
    return base.AsyncServerStreamingCall(request, context, continuation);
  }

  public override AsyncDuplexStreamingCall<TRequest, TResponse> AsyncDuplexStreamingCall<TRequest, TResponse>(
    ClientInterceptorContext<TRequest, TResponse> context,
    AsyncDuplexStreamingCallContinuation<TRequest, TResponse> continuation)
  {
    AddAuthCreds(context);
    return base.AsyncDuplexStreamingCall(context, continuation);
  }

  private void AddAuthCreds<TRequest, TResponse>(ClientInterceptorContext<TRequest, TResponse> context)
    where TRequest : class
    where TResponse : class
  {
    if (!string.IsNullOrEmpty(_token.CurrentAccessToken))
    {
      context.Options.Headers.Add("Authorization", $"Bearer {_token.CurrentAccessToken}");
    }
  }
}