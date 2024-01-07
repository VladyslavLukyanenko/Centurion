using Grpc.Core;
using Grpc.Core.Interceptors;
using Microsoft.Extensions.Logging;

namespace Centurion.SeedWork.Web.Foundation.Grpc.Interceptors;

public interface IScopeTransactionManager
{
  bool HasActiveTransactions { get; }
  ValueTask BeginTransactionAsync(CancellationToken ct = default);
  ValueTask Commit(CancellationToken ct = default);
  void CommitBlocking(CancellationToken ct = default);

  ValueTask Rollback(CancellationToken ct = default);
  void RollbackBlocking(CancellationToken ct = default);
}

public class TransactionScopeInterceptor : Interceptor
{
  private readonly ILogger<TransactionScopeInterceptor> _logger;
  private readonly IScopeTransactionManager _scopeTransactionManager;

  public TransactionScopeInterceptor(ILogger<TransactionScopeInterceptor> logger,
    IScopeTransactionManager scopeTransactionManager)
  {
    _logger = logger;
    _scopeTransactionManager = scopeTransactionManager;
  }


  public override TResponse BlockingUnaryCall<TRequest, TResponse>(TRequest request,
    ClientInterceptorContext<TRequest, TResponse> context,
    BlockingUnaryCallContinuation<TRequest, TResponse> continuation)
  {
    try
    {
      var r = base.BlockingUnaryCall(request, context, continuation);
      if (_scopeTransactionManager.HasActiveTransactions)
      {
        _scopeTransactionManager.CommitBlocking(context.Options.CancellationToken);
      }

      return r;
    }
    catch (Exception)
    {
      if (_scopeTransactionManager.HasActiveTransactions)
      {
        _scopeTransactionManager.RollbackBlocking(context.Options.CancellationToken);
      }

      throw;
    }
  }

  public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(TRequest request,
    ClientInterceptorContext<TRequest, TResponse> context,
    AsyncUnaryCallContinuation<TRequest, TResponse> continuation)
  {
    AsyncUnaryCall<TResponse> chamada = continuation(request, context);
    return new AsyncUnaryCall<TResponse>(
      TreatResponseUnique(chamada.ResponseAsync, context.Options.CancellationToken),
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
      TreatResponseUnique(chamada.ResponseAsync, context.Options.CancellationToken),
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
      new TxAwareResponseStream<TResponse>(chamada.ResponseStream, _scopeTransactionManager),
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
      new TxAwareResponseStream<TResponse>(chamada.ResponseStream, _scopeTransactionManager),
      chamada.ResponseHeadersAsync,
      chamada.GetStatus,
      chamada.GetTrailers,
      chamada.Dispose);
  }

  private async Task<TResponse> TreatResponseUnique<TResponse>(Task<TResponse> resposta, CancellationToken ct)
  {
    try
    {
      var r = await resposta;
      if (_scopeTransactionManager.HasActiveTransactions)
      {
        await _scopeTransactionManager.Commit(ct);
      }

      return r;
    }
    catch (Exception)
    {
      if (_scopeTransactionManager.HasActiveTransactions)
      {
        await _scopeTransactionManager.Rollback(ct);
      }

      throw;
    }
  }

  private class TxAwareResponseStream<TResponse> : IAsyncStreamReader<TResponse>
  {
    private readonly IAsyncStreamReader<TResponse> _stream;
    private readonly IScopeTransactionManager _scopeTransactionManager;

    public TxAwareResponseStream(IAsyncStreamReader<TResponse> stream, IScopeTransactionManager scopeTransactionManager)
    {
      _stream = stream;
      _scopeTransactionManager = scopeTransactionManager;
    }

    public TResponse Current => _stream.Current;

    public async Task<bool> MoveNext(CancellationToken ct)
    {
      try
      {
        var eof = await _stream.MoveNext(ct).ConfigureAwait(false);
        if (_scopeTransactionManager.HasActiveTransactions)
        {
          await _scopeTransactionManager.Commit(ct);
        }

        return eof;
      }
      catch (Exception)
      {
        if (_scopeTransactionManager.HasActiveTransactions)
        {
          await _scopeTransactionManager.Rollback(ct);
        }

        throw;
      }
    }
  }
}