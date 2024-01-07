using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace Centurion.Cli.Core.Services;

public class InMemoryTokenStore : ITokenStore
{
  private readonly BehaviorSubject<string?> _accessToken = new(null);

  public InMemoryTokenStore()
  {
    AccessToken = _accessToken.AsObservable()
      .DistinctUntilChanged();
  }

  public void UseToken(string accessToken)
  {
    _accessToken.OnNext(accessToken);
  }

  public IObservable<string?> AccessToken { get; }
  public string? CurrentAccessToken => _accessToken.Value;
}