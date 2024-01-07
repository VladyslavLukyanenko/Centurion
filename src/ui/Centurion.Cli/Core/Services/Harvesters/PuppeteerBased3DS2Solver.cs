using System.Collections.Concurrent;
using System.Text;
using System.Web;
using Centurion.Cli.Core.Domain;
using Centurion.Contracts.Checkout;
using PuppeteerSharp;
using Splat;

namespace Centurion.Cli.Core.Services.Harvesters;

public class PuppeteerBased3DS2Solver : I3DS2Solver
{
  private readonly IReadonlyDependencyResolver _resolver;
  private TaskCompletionSource<IDictionary<string, string>>? _completion;
  private static readonly ConcurrentDictionary<string, SemaphoreSlim> SolveGates = new();

  public PuppeteerBased3DS2Solver(IReadonlyDependencyResolver resolver)
  {
    _resolver = resolver;
  }

  public async ValueTask<Solve3DS2CommandReply> Solve(Solve3DS2Command solveParams)
  {
    var gate = SolveGates.GetOrAdd(solveParams.ProxyUrl, static _ => new SemaphoreSlim(1, 1));
    await using var puppeteerHandle = _resolver.GetService<IPuppeteerHandle>()!;
    CDPSession? cdpSession = null;
    try
    {
      await gate.WaitAsync(CancellationToken.None);
      await puppeteerHandle.Initialize(Proxy.Parse(solveParams.ProxyUrl), CancellationToken.None);


      cdpSession = puppeteerHandle.CdpSession;
      puppeteerHandle.BrowserProcess.Exited += OnBrowserProcessOnExited;
      cdpSession.MessageReceived += OnCdpSessionOnMessageReceived;

      await puppeteerHandle.Page.SetUserAgentAsync(solveParams.UserAgent);

      var html = ConstructForm(solveParams.FormMethod, solveParams.FormAction, solveParams.FormFields);
      var htmlBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(html));
      var formDataUrl = "data:text/html;base64," + htmlBase64;
      await puppeteerHandle.Page.GoToAsync(formDataUrl, new NavigationOptions { Timeout = 0 });

      _completion = new TaskCompletionSource<IDictionary<string, string>>();
      var payload = await _completion.Task;

      return new Solve3DS2CommandReply
      {
        Payload = { payload }
      };
    }
    finally
    {
      puppeteerHandle.BrowserProcess.Exited += OnBrowserProcessOnExited;
      if (cdpSession is not null)
      {
        cdpSession.MessageReceived += OnCdpSessionOnMessageReceived;
      }

      gate.Release();
    }

    void OnBrowserProcessOnExited(object? o, EventArgs eventArgs)
    {
      _ = Task.Run(async () =>
      {
        await DisposeAsync();
        Terminated?.Invoke(this, EventArgs.Empty);
      }, CancellationToken.None);
    }

    void OnCdpSessionOnMessageReceived(object? _, MessageEventArgs e) =>
      Task.Run(async () =>
      {
        if (e.MessageID == "Fetch.requestPaused")
        {
          var requestId = e.MessageData["requestId"]?.ToString();
          var request = e.MessageData["request"]!;
          var url = request["url"]!.ToString();
          var response = await cdpSession.SendAsync("Fetch.getResponseBody", new { requestId });
          var responseBodyStr = response["body"]!.ToString();
          var encoded = response["base64Encoded"]!.ToObject<bool>();

          if (encoded)
          {
            responseBodyStr = Encoding.UTF8.GetString(Convert.FromBase64String(responseBodyStr));
          }

          /*if (responseBodyStr.Contains("Error"))
          {
            _completion!.SetException(new Solve3DS2Exception());
          }
          else */
          var hasBody = request["hasPostData"]?.ToObject<bool>() ?? false;
          if (hasBody &&
              (url.Contains("callback/CREDIT_CARD")
               || url.Contains(solveParams.TermUrl, StringComparison.InvariantCultureIgnoreCase)))
          {
            var postedBody = request["postData"]!.ToObject<string>();
            var decodedBody = HttpUtility.UrlDecode(postedBody);
            var parsedPayload = HttpUtility.ParseQueryString(decodedBody);

            var payloadDict = parsedPayload.AllKeys.ToDictionary(k => k!, k => parsedPayload[k]!);

            _completion!.SetResult(payloadDict);
          }

          await cdpSession.SendAsync("Fetch.continueRequest", new { requestId });
        }
      }, CancellationToken.None);
  }

  private string ConstructForm(string method, string url, IDictionary<string, string> fields)
  {
    return $@"        <html>
            <body>
                <form method=""{method}"" action=""{url}"" id=""Cardinal-CCA-Form"">
                    <input type=""hidden"" name=""PaReq"" value=""{fields["PaReq"]}"" />
                    <input type=""hidden"" name=""MD"" value=""{fields["MD"]}"" />
                    <input type=""hidden"" name=""TermUrl"" value=""{fields["TermUrl"]}"" />
                </form>
                <script>
                    setTimeout(() => document.querySelector('#Cardinal-CCA-Form').submit(), 500);
                </script>
            </body>
        </html>";
  }

  public async ValueTask DisposeAsync()
  {
    _completion?.TrySetCanceled();
  }

  public event EventHandler? Terminated;
}

public class Solve3DS2Exception : Exception
{
  public Solve3DS2Exception()
    : base("Failed to process request")
  {
  }
}