using Centurion.TaskManager.Core;

namespace Centurion.TaskManager.Application;

public class MappedTask
{
  public MappedTask(CheckoutTask task, Product product)
  {
    Task = task;
    Product = product;
  }

  public CheckoutTask Task { get; private set; }
  public Product Product { get; private set; }
}