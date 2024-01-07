using System.ComponentModel;
using System.Diagnostics;
using System.IO.Compression;
using System.Net;
using System.Net.Http.Json;
using System.Net.Sockets;
using System.Reactive.Disposables;
using System.Security.Cryptography;
using System.Text;
using Centurion.Cli.Core.Config;
using Centurion.Cli.Core.Domain;
using Centurion.Net.Http;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using PuppeteerSharp;
using Titanium.Web.Proxy;
using Titanium.Web.Proxy.EventArguments;
using Titanium.Web.Proxy.Models;

namespace Centurion.Cli.Core.Services.Harvesters;

public class PuppeteerHandle : IPuppeteerHandle
{
  private readonly ILogger<PuppeteerHandle> _logger;
  private readonly IHttpClientFactory _httpClientFactory;
  private readonly HarvesterConfig _harvesterConfig;

  private Process? _browserProcess;
  private Page? _page;
  private CDPSession? _cdpSession;
  private Browser? _browser;
  private static readonly SemaphoreSlim GlobalGates = new(1, 1);
  private readonly CompositeDisposable _disposable = new();
  private readonly ILoggerFactory _loggerFactory;

  private readonly AsyncRetryPolicy<ChromeInfo?> _retrySocketErrorsPolicy = Policy<ChromeInfo?>
    .Handle<SocketException>()
    .Or<HttpRequestException>()
    .RetryForeverAsync();

  public PuppeteerHandle(ILogger<PuppeteerHandle> logger, IHttpClientFactory httpClientFactory,
    ILoggerFactory loggerFactory, HarvesterConfig harvesterConfig)
  {
    _logger = logger;
    _httpClientFactory = httpClientFactory;
    _loggerFactory = loggerFactory;
    _harvesterConfig = harvesterConfig;
  }

  public async ValueTask<Result> Initialize(Proxy proxy, CancellationToken ct)
  {
    var proxyUrl = proxy.ToUri().ToString();
    var userDataPath = await PrepareBrowser(proxyUrl);
    var chromeInfoCts = new CancellationTokenSource();
    var upstreamPort = StartLocalPasswordlessUpstreamProxyServer(proxy);
    var browserDebugPort = await StartBrowser(userDataPath, upstreamPort, chromeInfoCts, ct);

    var linkedCt = CancellationTokenSource.CreateLinkedTokenSource(chromeInfoCts.Token, ct).Token;
    try
    {
      await ConnectPuppeteer(browserDebugPort, linkedCt);
    }
    catch (TaskCanceledException exc) when (exc.CancellationToken.IsCancellationRequested)
    {
      const string error = "Browser process terminated. Please make all harvester windows closed.";
      return Result.Failure(error);
    }

    return Result.Success();
  }

  public async ValueTask DisposeAsync()
  {
    _disposable.Dispose();
    // if (_cdpSession is not null)
    // {
    // await _cdpSession.DetachAsync();
    // }

    if (_page is not null)
    {
      await _page.DisposeAsync();
    }

    if (_browser is not null)
    {
      await _browser.DisposeAsync();
    }

    _browserProcess?.Kill();
  }

  private async ValueTask<string> PrepareBrowser(string proxyUrl)
  {
    string userDataPath;
    try
    {
      await GlobalGates.WaitAsync(CancellationToken.None);
      EnsureBrowserUnpackedAndReady();
      var proxyHash = MD5.HashData(Encoding.UTF8.GetBytes(proxyUrl));
      var proxyHashStr = BitConverter.ToString(proxyHash).Replace("-", "").ToLowerInvariant();
      userDataPath = Path.Combine(_harvesterConfig.ChromiumBaseUserDir, proxyHashStr);
      if (!Directory.Exists(userDataPath))
      {
        Directory.CreateDirectory(userDataPath);
      }
    }
    finally
    {
      GlobalGates.Release();
    }

    return userDataPath;
  }

  private async ValueTask<int> StartBrowser(string userDataPath, int upstreamPort,
    CancellationTokenSource chromeInfoCts, CancellationToken ct)
  {
    int browserDebugPort = GenerateFreeRandomPort();
    string launchArg = string.Join(" ",
      $"--remote-debugging-port={browserDebugPort}",
      $"--user-data-dir=\"{userDataPath}\"",
      "--window-size=800,600",
      "--dark",
      "--no-default-browser-check",
      "--disable-session-crashed-bubble",
      "--no-first-run",
      "--lang=en",
      // "--app=\"about:blank\"",
      "--app=\"https://www.google.com\"",
      $"--proxy-server=\"localhost:{upstreamPort}\""
    );

    var startInfo = new ProcessStartInfo
    {
      Arguments = launchArg,
      FileName = _harvesterConfig.BrowserExePath,
      UseShellExecute = true,
      ErrorDialog = true
    };

    _browserProcess = new Process
    {
      StartInfo = startInfo,
      EnableRaisingEvents = true
    };

    try
    {
      if (!_browserProcess.Start())
      {
        throw new InvalidOperationException("Can't start controlled chrome instance");
      }

      _browserProcess.Exited += (_, _) => Task.Run(chromeInfoCts.Cancel, ct);
    }
    catch (Exception e) when (e is Win32Exception or InvalidOperationException)
    {
      chromeInfoCts.Cancel();
      await Task.Delay(TimeSpan.FromSeconds(1), CancellationToken.None);
      throw;
    }

    return browserDebugPort;
  }

  private async ValueTask ConnectPuppeteer(int browserDebugPort, CancellationToken ct)
  {
    using var httpClient = _httpClientFactory.CreateClient();
    string introspectionUrl = $"http://127.0.0.1:{browserDebugPort}/json/version";
    var chromeInfo = await _retrySocketErrorsPolicy.ExecuteAsync(chromeCt =>
      httpClient.GetFromJsonAsync<ChromeInfo>(introspectionUrl, chromeCt), ct);
    _browser = await Puppeteer.ConnectAsync(new ConnectOptions
    {
      BrowserWSEndpoint = chromeInfo!.WebSocketDebuggerUrl,
    }, _loggerFactory);

    for (Page[] pages; _page is null; pages = await _browser.PagesAsync(), _page = pages.FirstOrDefault())
    {
      await Task.Delay(TimeSpan.FromMilliseconds(200), ct);
    }

    _cdpSession = _page.Client;

    await _cdpSession.SendAsync("Fetch.enable", new
    {
      patterns = new object[]
      {
        new
        {
          requestStage = "Response"
        }
      }
    });
  }

  private int StartLocalPasswordlessUpstreamProxyServer(Proxy proxy)
  {
    var proxyServer = new ProxyServer
    {
      UpStreamHttpsProxy =
        new ExternalProxy(proxy.GetHostOrIp(), proxy.GetPort(), proxy.Username!, proxy.Password!),

      UpStreamHttpProxy =
        new ExternalProxy(proxy.GetHostOrIp(), proxy.GetPort(), proxy.Username!, proxy.Password!),
    };

    int localProxyPort = GenerateFreeRandomPort();
    var localProxyEndpoint = new ExplicitProxyEndPoint(IPAddress.Any, localProxyPort, false);

    Task OnLocalProxyEndpointOnBeforeTunnelConnectRequest(object _, TunnelConnectSessionEventArgs e)
    {
      e.DecryptSsl = false;
      return Task.CompletedTask;
    }

    localProxyEndpoint.BeforeTunnelConnectRequest += OnLocalProxyEndpointOnBeforeTunnelConnectRequest;

    proxyServer.AddEndPoint(localProxyEndpoint);
    proxyServer.Start();

    _disposable.Clear();
    _disposable.Add(new LazyDisposable(() =>
    {
      localProxyEndpoint.BeforeTunnelConnectRequest -= OnLocalProxyEndpointOnBeforeTunnelConnectRequest;
      proxyServer.Stop();
      proxyServer.Dispose();
    }));

    foreach (var endPoint in proxyServer.ProxyEndPoints)
    {
      _logger.LogDebug(
        "Local proxy listening on '{EndpointType}' endpoint at Ip {LocalProxyIp} and port: {LocalProxyPort}",
        endPoint.GetType().Name, endPoint.IpAddress, endPoint.Port);
    }

    return localProxyEndpoint.Port;
  }

  private void EnsureBrowserUnpackedAndReady()
  {
    if (Directory.Exists(_harvesterConfig.ChromiumDownloadDir))
    {
      return;
    }

    Directory.CreateDirectory(_harvesterConfig.ChromiumDownloadDir);
    ZipFile.ExtractToDirectory(_harvesterConfig.ChromeDistroArchiveLocation, _harvesterConfig.ChromiumDownloadDir);
  }

  private int GenerateFreeRandomPort()
  {
    var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
    try
    {
      var endPoint = new IPEndPoint(IPAddress.Any, 0);
      socket.Bind(endPoint);
      endPoint = (IPEndPoint)socket.LocalEndPoint!;
      return endPoint.Port;
    }
    finally
    {
      socket.Close();
      socket.Dispose();
    }
  }

  private void EnsureInitialized()
  {
    if (_page is null)
    {
      throw new InvalidOperationException("Harvester is not initialized yet.");
    }
  }

  public Page Page
  {
    get
    {
      EnsureInitialized();
      return _page!;
    }
  }

  public Browser Browser
  {
    get
    {
      EnsureInitialized();
      return _browser!;
    }
  }

  public CDPSession CdpSession
  {
    get
    {
      EnsureInitialized();
      return _cdpSession!;
    }
  }

  public Process BrowserProcess
  {
    get
    {
      EnsureInitialized();
      return _browserProcess!;
    }
  }

  private class ChromeInfo
  {
    public string WebSocketDebuggerUrl { get; set; } = null!;
  }
}