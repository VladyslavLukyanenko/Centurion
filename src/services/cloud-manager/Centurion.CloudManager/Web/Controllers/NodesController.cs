using Centurion.CloudManager.App.Model;
using Centurion.CloudManager.Domain;
using Centurion.CloudManager.Domain.Services;
using Centurion.CloudManager.Web.Commands;
using Centurion.SeedWork.Web.Foundation.Model;
using Centurion.TaskManager.Web.Foundation.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace Centurion.CloudManager.Web.Controllers;

public class NodesController : SecuredControllerBase
{
  private readonly IEnumerable<IInfrastructureClient> _infrastructureClients;

  public NodesController(IServiceProvider serviceProvider, IEnumerable<IInfrastructureClient> infrastructureClients)
    : base(serviceProvider)
  {
    _infrastructureClients = infrastructureClients;
  }

  [HttpGet]
  [ProducesResponseType(typeof(ApiContract<GroupedNodeList[]>), StatusCodes.Status200OK)]
  public IActionResult GetNodeList()
  {
    var list = _infrastructureClients.Select(_ => new GroupedNodeList
      {
        ProviderName = _.ProviderName,
        Nodes = _.AvailableNodes.ToList()
      })
      .ToArray();

    return Ok(list);
  }

  [HttpPost]
  [ProducesResponseType(typeof(ApiContract<long>), StatusCodes.Status200OK)]
  public async Task<IActionResult> StartOrRunAsync(StartOrRunNodeCommand cmd, CancellationToken ct)
  {
    var client = _infrastructureClients.FirstOrDefault(_ => _.ProviderName == cmd.ProviderName);
    if (client == null)
    {
      return BadRequest($"Invalid provider name '{cmd.ProviderName}'");
    }

    Node? node = null;
    if (!string.IsNullOrEmpty(cmd.StoppedNodeId))
    {
      node = client.GetStoppedById(cmd.StoppedNodeId);
      if (node == null)
      {
        return NotFound($"Node '{cmd.StoppedNodeId}' not found.");
      }
    }

    await client.RunOrStartAsync(node, ct);

    return Ok();
  }

  [HttpDelete("{provider}/{nodeId}/permanent")]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  public async Task<IActionResult> TerminateAsync(string provider, string nodeId, CancellationToken ct)
  {
    var client = _infrastructureClients.FirstOrDefault(_ => _.ProviderName == provider);
    if (client == null)
    {
      return NotFound();
    }

    var node = client.GetRunningById(nodeId) ?? client.GetStoppedById(nodeId);
    if (node == null)
    {
      return BadRequest("Node not found or is not running");
    }

    await client.TerminateAsync(node, ct);
    return NoContent();
  }

  [HttpDelete("{provider}/{nodeId}")]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  public async Task<IActionResult> StopAsync(string provider, string nodeId, CancellationToken ct)
  {
    var client = _infrastructureClients.FirstOrDefault(_ => _.ProviderName == provider);
    if (client == null)
    {
      return NotFound();
    }

    var node = client.GetRunningById(nodeId);
    if (node == null)
    {
      return BadRequest("Node not found or is not running");
    }

    await client.StopAsync(node, ct);
    return NoContent();
  }
}