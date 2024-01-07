using Centurion.TaskManager.Contracts;

// ReSharper disable once CheckNamespace
namespace Centurion.Contracts.Checkout;

public partial class InitializedCheckoutTaskData : ICheckoutTaskDataState
{
  string ICheckoutTaskDataState.ProductSku => Product.Sku;
}