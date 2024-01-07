using System.Net;
using Centurion.Monitor.Domain;

// ReSharper disable once CheckNamespace
namespace Centurion.Contracts.Monitor.Integration;

public partial class MonitoringStatusChanged
{
  public static MonitoringStatusChanged SessionNotPresent(MonitorTarget target) =>
    new(target.TaskId, target.Sku, target.Module, target.UserId, TaskStatusData.Amazon.SessionNotPresent);

  public static MonitoringStatusChanged GeneratingSession(MonitorTarget target) =>
    new(target.TaskId, target.Sku, target.Module, target.UserId, TaskStatusData.Amazon.GeneratingSession);

  public static MonitoringStatusChanged Monitoring(MonitorTarget target) =>
    new(target.TaskId, target.Sku, target.Module, target.UserId, TaskStatusData.Monitoring);

  public static MonitoringStatusChanged Antibot(MonitorTarget target) =>
    new(target.TaskId, target.Sku, target.Module, target.UserId, TaskStatusData.Amazon.Antibot);

  public static MonitoringStatusChanged ProxyBanned(MonitorTarget target) =>
    new(target.TaskId, target.Sku, target.Module, target.UserId, TaskStatusData.Amazon.ProxyBanned);

  public static MonitoringStatusChanged CaptchaDetected(MonitorTarget target) =>
    new(target.TaskId, target.Sku, target.Module, target.UserId, TaskStatusData.Amazon.CaptchaDetected);

  public static MonitoringStatusChanged UnknownErrorGeneratingSession(MonitorTarget target,
    HttpStatusCode statusCode) =>
    new(target.TaskId, target.Sku, target.Module, target.UserId,
      TaskStatusData.Amazon.UnknownErrorGeneratingSession(statusCode));

  public static MonitoringStatusChanged UnknownHttpErrorMonitor(MonitorTarget target,
    HttpStatusCode statusCode) =>
    new(target.TaskId, target.Sku, target.Module, target.UserId,
      TaskStatusData.Amazon.UnknownHttpErrorMonitor(statusCode));

  public static MonitoringStatusChanged UnknownErrorMonitor(MonitorTarget target) =>
    new(target.TaskId, target.Sku, target.Module, target.UserId, TaskStatusData.Amazon.UnknownErrorMonitor);

  public static MonitoringStatusChanged OutOfStock(MonitorTarget target) =>
    new(target.TaskId, target.Sku, target.Module, target.UserId, TaskStatusData.ProductOutOfStock);

  public static MonitoringStatusChanged InStock(MonitorTarget target) =>
    new(target.TaskId, target.Sku, target.Module, target.UserId, TaskStatusData.ProductInStock);
}