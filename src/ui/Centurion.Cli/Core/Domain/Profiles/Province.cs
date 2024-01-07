namespace Centurion.Cli.Core.Domain.Profiles;

public class Province
{
  public string Code { get; set; } = null!;
  public string Title { get; set; } = null!;

  public override string ToString()
  {
    return $"{nameof(Country)}({Title}, {Code})";
  }
}