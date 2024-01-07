using System.Runtime.Serialization;
using Centurion.Cli.Core.ViewModels;
using ReactiveUI.Fody.Helpers;

namespace Centurion.Cli.Core.Domain.Profiles;

[DataContract]
public class BillingModel : ViewModelBase
{
  [Reactive, DataMember] public string Cvv { get; set; } = null!;
  [Reactive, DataMember] public int ExpirationMonth { get; set; }
  [Reactive, DataMember] public int ExpirationYear { get; set; }
  [Reactive, DataMember] public string CardNumber { get; set; } = null!;
  [Reactive, DataMember] public string? HolderName { get; set; }
}