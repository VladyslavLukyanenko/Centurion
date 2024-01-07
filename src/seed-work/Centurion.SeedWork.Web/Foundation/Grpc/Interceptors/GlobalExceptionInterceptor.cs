using Grpc.Core;
using Grpc.Core.Interceptors;
using Microsoft.Extensions.Logging;

namespace Centurion.SeedWork.Web.Foundation.Grpc.Interceptors;

public class GlobalExceptionInterceptor : Interceptor
{
  private readonly ILogger<GlobalExceptionInterceptor> _logger;

  public GlobalExceptionInterceptor(ILogger<GlobalExceptionInterceptor> logger)
  {
    _logger = logger;
  }

  public override TResponse BlockingUnaryCall<TRequest, TResponse>(TRequest request,
    ClientInterceptorContext<TRequest, TResponse> context,
    BlockingUnaryCallContinuation<TRequest, TResponse> continuation)
  {
    try
    {
      return base.BlockingUnaryCall(request, context, continuation);
    }
    catch (Exception exp)
    {
      TreatException(exp);
      throw;
    }
  }

  public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(TRequest request,
    ClientInterceptorContext<TRequest, TResponse> context,
    AsyncUnaryCallContinuation<TRequest, TResponse> continuation)
  {
    AsyncUnaryCall<TResponse> chamada = continuation(request, context);
    return new AsyncUnaryCall<TResponse>(
      TreatResponseUnique(chamada.ResponseAsync),
      chamada.ResponseHeadersAsync,
      chamada.GetStatus,
      chamada.GetTrailers,
      chamada.Dispose);
  }

  public override AsyncClientStreamingCall<TRequest, TResponse> AsyncClientStreamingCall<TRequest, TResponse>(
    ClientInterceptorContext<TRequest, TResponse> context,
    AsyncClientStreamingCallContinuation<TRequest, TResponse> continuation)
  {
    AsyncClientStreamingCall<TRequest, TResponse> chamada = continuation(context);
    return new AsyncClientStreamingCall<TRequest, TResponse>(
      chamada.RequestStream,
      TreatResponseUnique(chamada.ResponseAsync),
      chamada.ResponseHeadersAsync,
      chamada.GetStatus,
      chamada.GetTrailers,
      chamada.Dispose);
  }

  public override AsyncServerStreamingCall<TResponse> AsyncServerStreamingCall<TRequest, TResponse>(TRequest request,
    ClientInterceptorContext<TRequest, TResponse> context,
    AsyncServerStreamingCallContinuation<TRequest, TResponse> continuation)
  {
    AsyncServerStreamingCall<TResponse> chamada = continuation(request, context);
    return new AsyncServerStreamingCall<TResponse>(
      new TreatResponseStream<TResponse>(chamada.ResponseStream, TreatException),
      chamada.ResponseHeadersAsync,
      chamada.GetStatus,
      chamada.GetTrailers,
      chamada.Dispose);
  }

  public override AsyncDuplexStreamingCall<TRequest, TResponse> AsyncDuplexStreamingCall<TRequest, TResponse>(
    ClientInterceptorContext<TRequest, TResponse> context,
    AsyncDuplexStreamingCallContinuation<TRequest, TResponse> continuation)
  {
    AsyncDuplexStreamingCall<TRequest, TResponse> chamada = continuation(context);
    return new AsyncDuplexStreamingCall<TRequest, TResponse>(
      chamada.RequestStream,
      new TreatResponseStream<TResponse>(chamada.ResponseStream, TreatException),
      chamada.ResponseHeadersAsync,
      chamada.GetStatus,
      chamada.GetTrailers,
      chamada.Dispose);
  }

  private void TreatException(Exception exc)
  {
    if (exc is RpcException)
    {
      return;
    }

    _logger.LogError(exc, "Unhandled error occurred");
    /*// Check if there's a trailer that we defined in the server
    if (!exc.Trailers.Any(x => x.Key.Equals("exception-bin")))
    {
      return;
    }

    // Convert exception from byte[] to  string
    string exceptionString = Encoding.UTF8.GetString(exc.Trailers.GetValueBytes("exception-bin"));

    // Convert string to exception
    Exception exception = JsonConvert.DeserializeObject<Exception>(exceptionString,
      new JsonSerializerSettings {TypeNameHandling = TypeNameHandling.Auto});

    // Required to keep the original stacktrace (https://stackoverflow.com/questions/66707139/how-to-throw-a-deserialized-exception)
    exception.GetType().GetField("_remoteStackTraceString", BindingFlags.NonPublic | BindingFlags.Instance)
      .SetValue(exception, exception.StackTrace);

    // Throw the original exception
    ExceptionDispatchInfo.Capture(exception).Throw();*/
  }

  private async Task<TResponse> TreatResponseUnique<TResponse>(Task<TResponse> resposta)
  {
    try
    {
      return await resposta;
    }
    catch (Exception exp)
    {
      TreatException(exp);
      throw;
    }
  }

  private class TreatResponseStream<TResponse> : IAsyncStreamReader<TResponse>
  {
    private readonly IAsyncStreamReader<TResponse> _stream;
    private readonly Action<Exception> _excHandler;

    public TreatResponseStream(IAsyncStreamReader<TResponse> stream, Action<Exception> excHandler)
    {
      _stream = stream;
      _excHandler = excHandler;
    }

    public TResponse Current => _stream.Current;

    public async Task<bool> MoveNext(CancellationToken cancellationToken)
    {
      try
      {
        return await _stream.MoveNext(cancellationToken).ConfigureAwait(false);
      }
      catch (Exception exp)
      {
        _excHandler(exp);
        throw;
      }
    }
  }
}