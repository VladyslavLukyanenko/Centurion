using Centurion.SeedWork.Primitives;

namespace Centurion.CloudManager.Domain;

public class UserInfo : ValueObject
{
  private UserInfo()
  {
  }

  public UserInfo(string id, string name)
  {
    Id = id;
    Name = name;
  }

  internal static UserInfo CreateEmpty() => new();

  public string Id { get; private set; } = null!;
  public string Name { get; private set; } = null!;

  protected override IEnumerable<object?> GetAtomicValues()
  {
    yield return Id;
  }
};