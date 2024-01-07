using System.Reactive.Disposables;
using System.Reactive.Linq;
using Centurion.Cli.Core.Domain.Profiles;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Centurion.Cli.Core.ViewModels.Profiles;

public class ProfileRowViewModel : ViewModelBase
{
  public ProfileRowViewModel(ProfileModel profile)
  {
    Profile = profile;

    var onChanges = profile.Changed.Select(_ => profile)
      .Merge(this.WhenAnyValue(_ => _.Profile));

    onChanges.Where(_ => _.Billing?.CardNumber.Length >= 4)
      .Select(_ => _.Billing?.CardNumber[^4..]) // BUG: throws when cardNumber is empty (occurs when pasting)
      .ToPropertyEx(this, _ => _.Last4Digits)
      .DisposeWith(Disposable);

    onChanges.Select(_ => _.Billing != null)
      .ToPropertyEx(this, _ => _.CreditCardAdded)
      .DisposeWith(Disposable);

    onChanges.Select(_ => _.FullName)
      .ToPropertyEx(this, _ => _.FullName)
      .DisposeWith(Disposable);
    onChanges.Select(_ => _.PhoneNumber)
      .ToPropertyEx(this, _ => _.PhoneNumber)
      .DisposeWith(Disposable);

    onChanges.Select(_ => _.Name)
      .ToPropertyEx(this, _ => _.ProfileName)
      .DisposeWith(Disposable);

    onChanges.Select(_ => _.ShippingAddress.Line2)
      .Select(line => string.IsNullOrEmpty(line) ? "<No Line2>" : line)
      .ToPropertyEx(this, _ => _.ShipAddressLine2)
      .DisposeWith(Disposable);
  }

  public ProfileModel Profile { get; }

  public bool CreditCardAdded { [ObservableAsProperty] get; } = default!;
  public string Last4Digits { [ObservableAsProperty] get; } = null!;
  public string ProfileName { [ObservableAsProperty] get; } = null!;
  public string ShipAddressLine2 { [ObservableAsProperty] get; } = null!;
  public string Email { [ObservableAsProperty] get; } = null!;
  public string FullName { [ObservableAsProperty] get; } = null!;
  public string PhoneNumber { [ObservableAsProperty] get; } = null!;
}