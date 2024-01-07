namespace Centurion.Cli.Core.Domain.Fields;

public interface IValueResolverConfigurationPresenter
{
  Task ShowConfigurationWindowAsync(string title, params Field[] configurationFields);
}