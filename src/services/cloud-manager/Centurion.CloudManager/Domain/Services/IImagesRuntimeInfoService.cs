namespace Centurion.CloudManager.Domain.Services;

public interface IImagesRuntimeInfoService
{
  Task RefreshStateAsync(IEnumerable<Node> nodes, CancellationToken ct = default);
}