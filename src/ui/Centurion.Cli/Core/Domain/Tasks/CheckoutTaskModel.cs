using Centurion.Cli.Core.Composition;
using Centurion.Cli.Core.ViewModels;
using Centurion.Contracts;
using NodaTime;
using ReactiveUI.Fody.Helpers;

namespace Centurion.Cli.Core.Domain.Tasks;

[TransientViewModel]
public class CheckoutTaskModel : ViewModelBase
{
  private const string DefaultPicture = "https://accounts-api.centurion.gg/CenturionLogo__small.png";

  public Guid Id { get; set; }
  public Instant UpdatedAt { get; init; }
  public Instant CreatedAt { get; init; }

  [Reactive] public ISet<Guid> ProfileIds { get; set; } = new HashSet<Guid>();
  [Reactive] public Module Module { get; set; }

  [Reactive] public Guid? CheckoutProxyPoolId { get; set; }

  [Reactive] public Guid? MonitorProxyPoolId { get; set; }
  [Reactive] public string ProductSku { get; set; } = null!;
  [Reactive] public string ProductPicture { get; set; } = DefaultPicture;
  [Reactive] public byte[] Config { get; set; } = Array.Empty<byte>();
}