using ReactiveUI;

namespace Centurion.Cli.Core.ViewModels;

public abstract class PageViewModelBase : ViewModelBase, IPageViewModel
{
  private readonly string _pageTitle;
  private readonly IMessageBus _messageBus;

  protected PageViewModelBase(string pageTitle, IMessageBus messageBus)
  {
    _pageTitle = pageTitle;
    _messageBus = messageBus;
  }

  public void PageActivated()
  {
    _messageBus.SendMessage(new ChangeHeaderTitle(_pageTitle));
    _messageBus.SendMessage(new ChangeHeaderContent(GetHeaderContent()));
  }

  protected virtual ViewModelBase? GetHeaderContent() => null;
}