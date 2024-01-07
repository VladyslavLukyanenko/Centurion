using System.Diagnostics;
using Centurion.Cli.Core.Domain;
using CSharpFunctionalExtensions;
using PuppeteerSharp;

namespace Centurion.Cli.Core.Services.Harvesters;

public interface IPuppeteerHandle : IAsyncDisposable
{
  ValueTask<Result> Initialize(Proxy proxy, CancellationToken ct);
    
  Page Page { get; }
  Browser Browser { get; }
  CDPSession CdpSession { get; }
  Process BrowserProcess { get; }
}