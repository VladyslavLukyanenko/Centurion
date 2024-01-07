#pragma warning disable 8618
using Centurion.Accounts.App.Config;
using Centurion.Accounts.Core.Products;

namespace Centurion.Accounts.Infra.Configs;

public class StripeGlobalConfig
{
  public string PaymentSuccessUrlTemplate { get; set; }
  public string PaymentCancelledUrlTemplate { get; set; }

  public string GetBillingDashboardReturnUrl(Dashboard dashboard, CommonConfig commonConfig) =>
    commonConfig.WebsiteUrl;

  public string GetPaymentSuccessfulUrl(Dashboard dashboard, CommonConfig commonConfig) =>
    commonConfig.WebsiteUrl + PaymentSuccessUrlTemplate;

  public string GetPaymentCancelledUrl(Dashboard dashboard, CommonConfig commonConfig) =>
    commonConfig.WebsiteUrl + PaymentCancelledUrlTemplate;
}