using System.Diagnostics;
using Centurion.Cli.Core.Domain.Accounts;
using Centurion.Cli.Core.Services.Sessions;
using Microsoft.Extensions.Logging;
using PuppeteerSharp;
using PuppeteerSharp.Input;

namespace Centurion.Cli.Core.Services;

public class PuppeteerBasedSessionAutoconfigurer : ISessionAutoconfigurator
{
  private readonly ILogger<PuppeteerBasedSessionAutoconfigurer> _logger;

  public PuppeteerBasedSessionAutoconfigurer(ILogger<PuppeteerBasedSessionAutoconfigurer> logger)
  {
    _logger = logger;
  }

  public event EventHandler<OneclickAutomationStatus>? StatusChanged;

  public async ValueTask<IList<string>> Configure(Account account, CancellationToken ct = default)
  {
    var fetcher = new BrowserFetcher();
    await fetcher.DownloadAsync(BrowserFetcher.DefaultChromiumRevision);
    await using var browser = await Puppeteer.LaunchAsync(new LaunchOptions
    {
      Headless = true
    });
    await using var page = await browser.NewPageAsync();
    try
    {
      await foreach (var status in TryLogin(page, account))
      {
        StatusChanged?.Invoke(this, status);
        var lvl = status.IsError ? LogLevel.Error : LogLevel.Information;
        _logger.Log(lvl, status.Message);
      }

      var cookies = await page.GetCookiesAsync();
      var cookiesStrList = cookies.Select(c => $"{c.Name}={c.Value}").ToList();
      _logger.LogInformation("Cookies collected: {@Cookies}", cookiesStrList);

      return cookiesStrList;
    }
    catch (Exception exc)
    {
      _logger.LogError(exc, "Failed to enable one-click");
      await ShowPic(page);
      throw;
    }
  }


  private static async IAsyncEnumerable<OneclickAutomationStatus> TryLogin(Page page, Account account)
  {
    // Start up the browser instance.
    string url = "https://www.amazon.com/";

    yield return "Opening Home page";

    await page.GoToAsync(url);
    await page.WaitForSelectorAsync("#nav-link-accountList");
    _ = page.ClickAsync("#nav-link-accountList");
    await page.WaitForNavigationAsync();
    yield return "Navigating to login page";

    // step: Enter phone num
    await page.WaitForSelectorAsync("#ap_email");
    await page.FocusAsync("#ap_email");
    await page.Keyboard.TypeAsync(account.Email, new TypeOptions { Delay = 50 });
    yield return "Entered login";
    _ = page.ClickAsync("#continue");
    await page.WaitForNavigationAsync();

    // step: Enter pwd
    await page.WaitForSelectorAsync("#ap_password");
    await page.FocusAsync("#ap_password");
    await page.Keyboard.TypeAsync(account.Password, new TypeOptions { Delay = 50 });
    await page.ClickAsync("[name=rememberMe]");
    yield return "Entered password. Navigating to settings page";
    _ = page.ClickAsync("#signInSubmit");
    await page.WaitForNavigationAsync();

    // step: Open Settings page
    await page.GoToAsync(url);
    await page.WaitForSelectorAsync("#nav-link-accountList");
    _ = page.ClickAsync("#nav-link-accountList");
    await page.WaitForNavigationAsync();
    yield return "Navigating to payments page";

    // step: Open payment methods page
    await page.WaitForSelectorAsync("[data-card-identifier=PaymentOptions]");
    _ = page.ClickAsync("[data-card-identifier=PaymentOptions]");
    await page.WaitForNavigationAsync();

    // step: Check payment method added
    await page.WaitForSelectorAsync(".apx-wallet-tab-set-header-row");

    var paymentMethodAdded =
      await page.QuerySelectorAsync(
        ".apx-wallet-tab-set-header-row + .apx-wallet-desktop-payment-method-selectable-tab-css") is not null;
    if (!paymentMethodAdded)
    {
      yield return OneclickAutomationStatus.Error(
        "No payment method was added. Please add one, make it as default and try again");
      yield break;
    }

    yield return "Payment method detected. Checking settings...";

    // step: Enable one-click payment
    _ = page.ClickAsync("a[href*='/settings']");
    await page.WaitForNavigationAsync();
    await page.WaitForSelectorAsync("a[href*='/manageoneclick']");
    _ = page.ClickAsync("a[href*='/manageoneclick']");
    await page.WaitForNavigationAsync();

    var isEnabled =
      await page.EvaluateExpressionAsync<bool>(
        "document.querySelector(\"#pmts-toggle-this-browser\") && document.querySelector(\"#pmts-toggle-this-browser\").checked");

    if (isEnabled)
    {
      yield return "One click already configured";
      yield break;
    }

    var isDefaultPaymentSelected = await page.QuerySelectorAsync(
      "form[action*='/manageoneclick'] .a-icon.a-icon-success.a-icon-small") is not null;
    if (!isDefaultPaymentSelected)
    {
      yield return OneclickAutomationStatus.Error(
        "No default payment method was selected. Please fix it and try again");
    }

    var canBeEnabled =
      await page.QuerySelectorAsync("form.pmts-portal-component[data-pmts-component-id] .a-switch-label") is not null;
    if (!canBeEnabled)
    {
      await ShowPic(page);
      yield return OneclickAutomationStatus.Error("Can't enable one-click payment.");
      yield break;
    }

    await page.ClickAsync("form.pmts-portal-component[data-pmts-component-id] .a-switch-label");
    await page.WaitForResponseAsync(r => r.Url.Contains("UpdateBrowserOneClickStatusEvent"));
    yield return "One click configured";

#if DEBUG
    await ShowPic(page);
#endif
  }

  private static async Task ShowPic(Page page)
  {
    var tmp = Path.GetTempFileName() + ".png";
    await page.ScreenshotAsync(tmp);
    new Process
    {
      StartInfo = new ProcessStartInfo(tmp)
      {
        UseShellExecute = true
      }
    }.Start();
  }
}