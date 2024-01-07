using Centurion.Contracts.Checkout;
using Centurion.TaskManager.Core.Services;

namespace Centurion.TaskManager.Web.Services;

public interface ICheckoutClientFactory
{
  Checkout.CheckoutClient Create(ICloudConnection cloudConnection);
}