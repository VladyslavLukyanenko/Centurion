using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Text.RegularExpressions;
using Centurion.Cli.Core.Domain;
using Centurion.Cli.Core.Domain.Accounts;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using PuppeteerSharp;
using PuppeteerSharp.Input;

namespace Centurion.Cli.Core.Services.Harvesters;

public class PuppeteerBasedHarvester : IHarvester
{
  private static readonly Regex CaptchaTokenReplaceContainerPattern =
    new("<main .*?</main>", RegexOptions.Compiled | RegexOptions.Multiline);

  private readonly IPuppeteerHandle _puppeteerHandle;
  private readonly SemaphoreSlim _gates = new(1, 1);
  private readonly SemaphoreSlim _incrementGates = new(1, 1);
  private TaskCompletionSource<string>? _tokenHandler;
  private readonly ILogger<PuppeteerBasedHarvester> _logger;
  private InitializedHarvesterModel? _harvester;
  private readonly BehaviorSubject<int> _tokensHarvested = new(0);

  private const string LoginUrl =
    "https://accounts.google.com/signin/v2/identifier?service=mail&passive=true&rm=false&continue=https://mail.google.com/mail/&ss=1&scc=1&ltmpl=default&ltmplcache=2&emr=1&osid=1&flowName=GlifWebSignIn&flowEntry=ServiceLogin";

  public PuppeteerBasedHarvester(IPuppeteerHandle puppeteerHandle, ILogger<PuppeteerBasedHarvester> logger)
  {
    _puppeteerHandle = puppeteerHandle;
    _logger = logger;
    TokensHarvested = _tokensHarvested.AsObservable();
  }

  public async ValueTask<Result> Start(InitializedHarvesterModel harvester, CancellationToken ct)
  {
    _harvester = harvester;
    const string siteKey = "6Lf34M8ZAAAAANgE72rhfideXH21Lab333mdd2d-";
    const string action = "yzysply_wr_pageview";
    try
    {
      await _gates.WaitAsync(CancellationToken.None);

      var result = await _puppeteerHandle.Initialize(harvester.Proxy, ct);
      if (result.IsFailure)
      {
        return result;
      }

      _puppeteerHandle.BrowserProcess.Exited += (_, _) =>
      {
        _ = Task.Run(async () =>
        {
          await DisposeAsync();
          Terminated?.Invoke(this, EventArgs.Empty);
        }, CancellationToken.None);
      };

      var page = _puppeteerHandle.Page;
      page.Console += (_, e) =>
      {
        if (_tokenHandler is not null && e.Message.Text.StartsWith("03"))
        {
          _tokenHandler.SetResult(e.Message.Text);
          _logger.LogDebug("Solved v3 captcha {Token}", e.Message.Text);
          try
          {
            _incrementGates.Wait(CancellationToken.None);
            _tokensHarvested.OnNext(_tokensHarvested.Value + 1);
          }
          finally
          {
            _incrementGates.Release();
          }
        }
      };

      _puppeteerHandle.CdpSession.MessageReceived += async (_, e) =>
      {
        try
        {
          if (e.MessageID == "Fetch.requestPaused")
          {
            await ProcessRequestPausedMessage(e, siteKey, action);
          }
        }
        catch (Exception exc)
        {
          _logger.LogError(exc, "Failed to process puppeteer message");
        }
      };

      await Task.Delay(TimeSpan.FromSeconds(2), ct);
      await page.GoToAsync(LoginUrl, new NavigationOptions { Timeout = 0 });
      await Task.Delay(TimeSpan.FromSeconds(2), ct);

      if (await page.EvaluateExpressionAsync<bool>(
            "!!document.querySelector(\".button--mobile-before-hero-only[data-action='sign in']\")"))
      {
        await page.ClickAsync(".button--mobile-before-hero-only[data-action='sign in']");

        await page.WaitForSelectorAsync("[name=identifier]", new WaitForSelectorOptions { Timeout = 0 });

        if (await page.QuerySelectorAsync("[name=identifier]") != null)
        {
          await page.TypeAsync("[name=identifier]", harvester.Account.Email,
            new TypeOptions { Delay = (int)TimeSpan.FromMilliseconds(75).TotalMilliseconds });
        }

        if (!await page.EvaluateExpressionAsync<bool>(
              "(document && document.body && document.body.innerHTML || '').toString().indexOf('Compose') > 0"))
        {
          await page.WaitForExpressionAsync(
            "(document && document.body && document.body.innerHTML || '').toString().indexOf('Compose') > 0",
            new WaitForFunctionOptions { Timeout = 0 });
        }

        await page.GoToAsync("https://www.google.com/search?q=youtube+videos",
          new NavigationOptions { Timeout = (int)TimeSpan.FromSeconds(45).TotalMilliseconds });
        await Task.Delay(TimeSpan.FromSeconds(2), ct);
      }

      await page.EvaluateExpressionAsync("location.href = \"https://www.youtube.com/\"");
      IsInitialized = true;

      return Result.Success();
    }
    finally
    {
      _gates.Release();
    }
  }

  private async Task ProcessRequestPausedMessage(MessageEventArgs e, string siteKey, string siteAction)
  {
    var requestId = e.MessageData["requestId"]?.ToString();
    var url = e.MessageData["request"]!["url"]!.ToString();

    var isIcon = url.Contains(".ico");
    var netwError = e.MessageData["responseErrorReason"];
    // var method = e.MessageData["request"]!["method"]?.ToString();

    var responseCode = e.MessageData["responseStatusCode"]?.ToObject<int>();
    var cdpSession = _puppeteerHandle.CdpSession;
    if (_tokenHandler is null || netwError is not null || responseCode == 302 || isIcon)
    {
      await cdpSession.SendAsync("Fetch.continueRequest", new { requestId });
      return;
    }

    var response = await cdpSession.SendAsync("Fetch.getResponseBody", new { requestId });
    var responseBodyStr = response["body"]!.ToString();
    var encoded = response["base64Encoded"]!.ToObject<bool>();
    var responseBody = encoded
      ? Convert.FromBase64String(responseBodyStr)
      : Encoding.UTF8.GetBytes(responseBodyStr);

    var page = _puppeteerHandle.Page;
    if (url.Contains("recaptcha/api2/bframe"))
    {
      await cdpSession.SendAsync("Fetch.continueRequest", new { requestId });
      var frameIx = url.Contains("atmos-tokyo") ? 1 : 2;
      var frame = page.Frames.ElementAt(frameIx);
      await frame.EvaluateExpressionAsync(@"delay = ms => new Promise(resolve => setTimeout(resolve, ms));

~function start() {
    delay(200).then(() => {
        document.querySelector('#recaptcha-anchor').click();
    })
}()");
      return;
    }

    if (url.Contains("/recaptcha/api2/userverify?k="))
    {
      var str = Encoding.UTF8.GetString(responseBody);
      if (str.StartsWith(")]}'\n[\"uvresp\",\"03") && str.EndsWith("\"]"))
      {
        var token = str.Substring(str.IndexOf("03", StringComparison.Ordinal));
        token = token[..token.IndexOf("\"", StringComparison.Ordinal)];
        _tokenHandler.SetResult(token);
      }

      await cdpSession.SendAsync("Fetch.continueRequest", new { requestId });
      return;
    }

    if (url.Contains("anchor"))
    {
      await cdpSession.SendAsync("Fetch.continueRequest", new { requestId });
      if (!await page.EvaluateExpressionAsync<bool>("'completion' in window"))
      {
        await page.ExposeFunctionAsync<string, string>("completion", token =>
        {
          _tokenHandler.SetResult(token);
          return token;
        });
      }

      return;
    }


    if (IsGoogleJsOrCheckouts(url))
    {
      responseBody = Encoding.UTF8.GetBytes(GetReplacementHtmlTemplate());
    }
    else if (IsGoogleOrNonYeezyFinishLine(url))
    {
      var harvesterIx = 0;
      responseBody = Encoding.UTF8.GetBytes(GetTokenConsoleLogHtmlTemplate(harvesterIx, siteKey, siteAction));
    }
    else if (IsGoogleScriptOrCheckpoint(url))
    {
      responseBody = Encoding.UTF8.GetBytes(BuildCaptchaInterceptorHtmlReplacer(url));
    }

    if (encoded)
    {
      responseBodyStr = Convert.ToBase64String(responseBody);
      // int mod4 = responseBody.Length % 4;
      // if (mod4 > 0)
      // {
      //   responseBody += new string('=', 4 - mod4);
      // }
    }

    var responseHeaders = e.MessageData["responseHeaders"];
    await cdpSession.SendAsync("Fetch.fulfillRequest", new
    {
      requestId,
      responseHeaders,
      responseCode,
      body = responseBodyStr
    });
  }

  public async ValueTask<string> TrySolveCaptcha(string url, TimeSpan? timeout = null)
  {
    var page = _puppeteerHandle.Page;
    try
    {
      await _gates.WaitAsync();
      _tokenHandler = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
      await page.GoToAsync(url);

      var result = await _tokenHandler.Task;
      _tokenHandler = null;
      return result;
    }
    finally
    {
      _gates.Release();
    }
  }

  public IObservable<int> TokensHarvested { get; }
  public Proxy Proxy => _harvester?.Proxy ?? throw new InvalidOperationException("Not initialized");
  public Account Account => _harvester?.Account ?? throw new InvalidOperationException("Not initialized");
  public HarvesterModel Harvester => _harvester?.Harvester ?? throw new InvalidOperationException("Not initialized");

  public bool IsInitialized { get; private set; }

  public event EventHandler? Terminated;

  public async ValueTask DisposeAsync()
  {
    _tokenHandler?.TrySetCanceled();
    await _puppeteerHandle.DisposeAsync();
  }

  private static bool IsGoogleScriptOrCheckpoint(string url)
  {
    return url.Contains("https://www.google.com") && url.EndsWith(".js")
           || url.Contains("/checkpoint");
  }

  private static string BuildCaptchaInterceptorHtmlReplacer(string url)
  {
    var match = CaptchaTokenReplaceContainerPattern.Match(url);
    if (!match.Success)
    {
      return "";
    }

    string content = match.Groups[0].Value;
    if (content.Contains("easylockdown-content"))
    {
      content = content.Split("<div class='easylockdown-content' style='display:none;'>")[1];
      if (content.Contains("<div id=\"easylockdown-password-form\" style=\"display: none;\">"))
      {
        content = content.Split("<div id=\"easylockdown-password-form\" style=\"display: none;\">")[0];
      }
    }

    content = content.Replace("var onCaptchaSuccess = function() {",
      "var onCaptchaSuccess = function() { window.completion(grecaptcha.getResponse()); ");
    content = content.Replace("(?s)<button type=\"submit\" class=\"btn\">.*?</button>", "");
    return $@"<!doctype html>
<html class=""no-js"" lang=""en"">
{content}
</html>";
  }

  private static bool IsGoogleOrNonYeezyFinishLine(string url)
  {
    return url.Contains("https://www.google.com") && url.EndsWith(".js")
           || url.Contains("yeezysupply")
           || url.Contains("jdsports")
           || url.Contains("finishline")
           || url.EndsWith("/account/register");
  }

  private static bool IsGoogleJsOrCheckouts(string url)
  {
    if (url.Contains("https://www.google.com") && url.EndsWith(".js"))
    {
      return true;
    }

    return url.Contains("/checkouts/");
  }

  private static string GetTokenConsoleLogHtmlTemplate(int harvesterIx, string sitekey, string action)
  {
    return @$"<html>
<body style=""background-color:#050A2A"">
<header>
    <h1 style=""color:#E4C983;"">Centurion Solutions. Instance: {harvesterIx}</span> </h1>
</header>
<main>
    <script src=""https://www.google.com/recaptcha/enterprise.js?render={sitekey}""></script>
    <script>
        grecaptcha.enterprise.ready(function() {{
            grecaptcha.enterprise.execute('{sitekey}', {{action: '{action}'}}).then(function(token) {{
                console.log(token);
            }});
        }});
    </script>
</main>
</body>
</html>";
  }

  private static string GetReplacementHtmlTemplate()
  {
    return @"
<!DOCTYPE html>
<head>
</head>
<body>
<div class=""content"" data-content>
    <div class=""wrap"">
        <div class=""main"">
            <main class=""main__content"" role=""main"">
                <div class=""step"" data-step=""contact_information"" data-last-step=""false"">
                    <form class=""edit_checkout"" novalidate=""novalidate"" data-customer-information-form=""true"" data-email-or-phone=""false"" action=""/3623944241/checkouts/1820e440965a6c58cde8496030b915f2"" accept-charset=""UTF-8"" method=""post""><input type=""hidden"" name=""_method"" value=""patch"" /><input type=""hidden"" name=""authenticity_token"" value=""35jINpr1Cut0hwPjNqJj6fnF2b53FAG2HMUUuEJOud3IAebeC+nvIOTZFlaUmqFbWPEKqel5DS8haChbffQTBA=="" />
                        <div class=""step__sections"">
                            <div class=""section"">
                                <div class=""section__content"">
                                    <div class=""fieldset"">
                                        <div class=""field field--required"">
                                            <script>
                                                //<![CDATA[
                                                var onCaptchaSuccess = function() {
                                                    var event;

                                                    try {
                                                        event = new Event('captchaSuccess', {bubbles: true, cancelable: true});
                                                    } catch (e) {
                                                        event = document.createEvent('Event');
                                                        event.initEvent('captchaSuccess', true, true);
                                                    }

                                                    window.dispatchEvent(event);
                                                }

                                                //]]>
                                            </script>
                                            <script>
                                                //<![CDATA[
                                                window.addEventListener('captchaSuccess', function() {
                                                    var responseInput = document.querySelector('.g-recaptcha-response');
                                                    var submitButton = document.querySelector('.dialog-submit');

                                                    if (submitButton instanceof HTMLElement) {
                                                        var needResponse = (responseInput instanceof HTMLElement);
                                                        var responseValueMissing = !responseInput.value;
                                                        submitButton.disabled = (needResponse && responseValueMissing);
                                                    }
                                                }, false);

                                                //]]>
                                            </script>
                                            <script>
                                                //<![CDATA[
                                                var recaptchaCallback = function() {
                                                    grecaptcha.render('g-recaptcha', {
                                                        sitekey: ""6LeoeSkTAAAAAA9rkZs5oS82l69OEYjKRZAiKdaF"",
                                                        size: (window.innerWidth > 320) ? 'normal' : 'compact',
                                                        callback: 'onCaptchaSuccess',
                                                        s: 'SZzKBEkYSrZrqM63_nN6HalJe6MX5WGxkZ5NDB6xBO1mLmNTw6Tufntx-1l6Ff5l2DeEncP2QNtJ5q7dZfrXunPLHTq_wvTkS2N7CJwiX_u3pKGLK6Dys00IjlKYf2bLipSbj-2ThOhzvgNvBOWGvfXlIN59VRkckWbS0ohZSKdnOYC_UL_eLIzQErTj-3TyjlHpvGZ8bRRErCHxFVcaoK-s4LNUOB4mN3Sl3VDLcYRS0CDa9AxnZF_sCDwSaVZBqpbEVxeSx7t1jJHteTS1-bs_XYNzLDwWGI0TsG8aKiqTIhzlK-swHbtJmqkgBUFQQiqO5v-Q7hka-4D4ODmdK8zWN4NgcuOTRLot5GAgyha-q_KtUVE',
                                                    });
                                                };

                                                //]]>
                                            </script>
                                            <script src=""https://www.recaptcha.net/recaptcha/api.js?onload=recaptchaCallback&amp;render=6LcCR2cUAAAAANS1Gpq_mDIJ2pQuJphsSQaUEuc9&amp;hl=en"" async=""async"">
                                                //<![CDATA[

                                                //]]>
                                            </script>
                                            <div id=""g-recaptcha"" class=""g-recaptcha""></div>
                                        </div>    </div>
                                </div>
                            </div>
                        </div>
                    </form>
                </div>
            </main>
        </div>
    </div>
</div>
</body>
</html>
";
  }
}