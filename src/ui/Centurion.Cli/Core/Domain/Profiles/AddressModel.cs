using System.Runtime.Serialization;
using Centurion.Cli.Core.ViewModels;
using ReactiveUI.Fody.Helpers;

namespace Centurion.Cli.Core.Domain.Profiles;

[DataContract]
public class AddressModel : ViewModelBase
{
  [Reactive, DataMember] public string Line1 { get; set; } = null!;
  [Reactive, DataMember] public string Line2 { get; set; } = null!;

  [Reactive, DataMember] public string City { get; set; } = null!;
  [Reactive, DataMember] public string ZipCode { get; set; } = null!;
  [Reactive, DataMember] public string CountryId { get; set; } = null!;
  [Reactive, DataMember] public string? ProvinceCode { get; set; }
}