using System.Runtime.Serialization;
using Centurion.Cli.Core.ViewModels;
using Centurion.SeedWork.Primitives;
using DynamicData.Binding;
using ReactiveUI.Fody.Helpers;

namespace Centurion.Cli.Core.Domain.Profiles;

[DataContract]
public class ProfileGroupModel : ViewModelBase, IEntity<Guid>, IEquatable<ProfileGroupModel>
{
  public Guid Id { get; init; }
  [Reactive, DataMember] public string Name { get; init; } = null!;

  [DataMember] public ObservableCollectionExtended<ProfileModel> Profiles { get; private set; } = new();

  public bool Equals(ProfileGroupModel? other)
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

    return Equals((ProfileGroupModel)obj);
  }

  public override int GetHashCode()
  {
    return Id.GetHashCode();
  }

  public static bool operator ==(ProfileGroupModel? left, ProfileGroupModel? right)
  {
    return Equals(left, right);
  }

  public static bool operator !=(ProfileGroupModel? left, ProfileGroupModel? right)
  {
    return !Equals(left, right);
  }
}