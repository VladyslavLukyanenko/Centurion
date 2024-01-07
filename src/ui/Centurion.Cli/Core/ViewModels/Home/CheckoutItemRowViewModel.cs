using Centurion.Contracts.Analytics;
using ReactiveUI.Fody.Helpers;

namespace Centurion.Cli.Core.ViewModels.Home;

public class CheckoutItemRowViewModel : ViewModelBase
{
  public CheckoutItemRowViewModel(CheckoutInfoData item)
  {
    Item = item;
  }

  [Reactive] public CheckoutInfoData Item { get; private set; }
}