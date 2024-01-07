using CSharpFunctionalExtensions;
using MassTransit;
using Centurion.Accounts.App.ChargeBackers.Services;
using Centurion.Accounts.Core.ChargeBackers.Events;
using Centurion.Accounts.Core.ChargeBackers.Services;
using Centurion.Accounts.Core.Primitives;
using Centurion.Accounts.Core.Products.Events.Dashboards;
using Centurion.Accounts.Core.Products.Services;

namespace Centurion.Accounts.Infra.ChargeBackers.Consumers;

public class ChargeBackersExportConsumer : IConsumer<ChargeBackerCreated>, IConsumer<ChargeBackersExportToggled>
{
  private readonly IDashboardRepository _dashboardRepository;
  private readonly IChargeBackerRepository _chargeBackerRepository;
  private readonly IChargeBackerExportService _chargeBackerExportService;
  private readonly IUnitOfWork _unitOfWork;

  public ChargeBackersExportConsumer(IDashboardRepository dashboardRepository,
    IChargeBackerRepository chargeBackerRepository, IChargeBackerExportService chargeBackerExportService,
    IUnitOfWork unitOfWork)
  {
    _dashboardRepository = dashboardRepository;
    _chargeBackerRepository = chargeBackerRepository;
    _chargeBackerExportService = chargeBackerExportService;
    _unitOfWork = unitOfWork;
  }

  public async Task Consume(ConsumeContext<ChargeBackerCreated> context)
  {
    var ct = context.CancellationToken;
    var m = context.Message;
    var dashboard = await _dashboardRepository.GetByIdAsync(m.DashboardId, ct);
    if (dashboard == null || !dashboard.ChargeBackersExportEnabled)
    {
      return;
    }

    var chargeBacker = await _chargeBackerRepository.GetByIdAsync(m.ChargeBackerId, ct);
    if (chargeBacker == null)
    {
      return;
    }

    var result = await _chargeBackerExportService.ExportAsync(chargeBacker, dashboard, ct);
    await result.OnSuccessTry(async () => await _unitOfWork.SaveEntitiesAsync(ct));
  }

  public async Task Consume(ConsumeContext<ChargeBackersExportToggled> context)
  {
    var ct = context.CancellationToken;
    var m = context.Message;
    if (!m.IsEnabled)
    {
      return;
    }

    var dashboard = await _dashboardRepository.GetByIdAsync(m.DashboardId, ct);
    if (dashboard == null || !dashboard.ChargeBackersExportEnabled)
    {
      return;
    }

    var notExportedChargeBackers = await _chargeBackerRepository.GetNotExportedAsync(dashboard.Id, ct);
    foreach (var chargeBacker in notExportedChargeBackers)
    {
      await _chargeBackerExportService.ExportAsync(chargeBacker, dashboard, ct);
    }

    await _unitOfWork.SaveEntitiesAsync(ct);
  }
}