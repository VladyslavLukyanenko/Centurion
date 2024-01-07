using System.Linq.Expressions;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Runtime.Serialization;
using Centurion.Cli.Core.ViewModels;
using Centurion.SeedWork.Primitives;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Centurion.Cli.Core.Domain.Profiles;

[DataContract]
public class ProfileModel : ViewModelBase, IEntity<Guid>, IEquatable<ProfileModel>
{
  public ProfileModel()
  {
    DispatchOnChanges(_ => _.ShippingAddress, nameof(ShippingAddress));
    DispatchOnChanges(_ => _.BillingAddress, nameof(BillingAddress));
    DispatchOnChanges(_ => _.Billing, nameof(Billing));
    this.WhenAnyValue(_ => _.FirstName)
      .CombineLatest(this.WhenAnyValue(_ => _.LastName), (fname, lname) => $"{fname} {lname}")
      .DistinctUntilChanged()
      .ToPropertyEx(this, _ => _.FullName);

    void DispatchOnChanges<T>(Expression<Func<ProfileModel, T?>> selector, string changedPropName)
      where T : ViewModelBase
    {
      IDisposable? onChanged = null;
      this.WhenAnyValue(selector).Subscribe(s =>
        {
          onChanged?.Dispose();
          onChanged = s?.Changed.Subscribe(_ => this.RaisePropertyChanged(changedPropName));
        })
        .DisposeWith(Disposable);
    }
  }

  public Guid Id { get; set; } = Guid.NewGuid();
  [Reactive, DataMember] public string Name { get; set; } = null!;
  [Reactive, DataMember] public string Email { get; set; } = null!;
  [Reactive, DataMember] public string FirstName { get; set; } = null!;
  [Reactive, DataMember] public string LastName { get; set; } = null!;
  public string FullName { [ObservableAsProperty] get; } = null!;

  [Reactive, DataMember] public AddressModel BillingAddress { get; set; } = new();
  [Reactive, DataMember] public AddressModel ShippingAddress { get; set; } = new();
  [Reactive, DataMember] public string PhoneNumber { get; set; } = null!;

  [Reactive, DataMember] public BillingModel? Billing { get; set; }
  [Reactive, DataMember] public bool BillingAsShipping { get; set; } = true;

  public bool Equals(ProfileModel? other)
  {
    if (ReferenceEquals(null, other))
    {
      return false;
    }

    if (ReferenceEquals(this, other))
    {
      return true;
    }

    return Id.Equals(other.Id);
  }

  public override bool Equals(object? obj)
  {
    if (ReferenceEquals(null, obj))
    {
      return false;
    }

    if (ReferenceEquals(this, obj))
    {
      return true;
    }

    if (obj.GetType() != this.GetType())
    {
      return false;
    }

    return Equals((ProfileModel)obj);
  }

  public override int GetHashCode()
  {
    return Id.GetHashCode();
  }

  public static bool operator ==(ProfileModel? left, ProfileModel? right)
  {
    return Equals(left, right);
  }

  public static bool operator !=(ProfileModel? left, ProfileModel? right)
  {
    return !Equals(left, right);
  }
}