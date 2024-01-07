namespace Centurion.Cli.Core.ViewModels;

public class ChangeHeaderContent
{
  public ChangeHeaderContent(ViewModelBase? content)
  {
    Content = content;
  }

  public ViewModelBase? Content { get; }
}