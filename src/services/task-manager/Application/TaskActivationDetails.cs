using Centurion.TaskManager.Core;

namespace Centurion.TaskManager.Application;

public class TaskActivationDetails
{
  public TaskActivationDetails(MappedTask task, ActivatedTask activatedTask)
  {
    ActivatedTask = activatedTask;
    Task = task.Task;
    Product = task.Product;
  }

  public CheckoutTask Task { get; private set; }
  public Product Product { get; private set; }
  public ActivatedTask ActivatedTask { get; private set; }
}