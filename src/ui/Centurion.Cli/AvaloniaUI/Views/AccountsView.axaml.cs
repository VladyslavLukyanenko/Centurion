using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Centurion.Cli.Core.ViewModels.Accounts;

namespace Centurion.Cli.AvaloniaUI.Views;

public class AccountsView : ReactiveUserControl<AccountsViewModel>
{
  public AccountsView()
  {
    InitializeComponent();
  }

  private void InitializeComponent()
  {
    AvaloniaXamlLoader.Load(this);
  }
}