namespace Centurion.Cli.Core.Domain.Profiles;

public class Country
{
  public string Id { get; set; } = null!;
  public string Title { get; set; } = null!;
  public bool IsProvincesRequired { get; set; }
  public bool IsProvincesList { get; set; }
  public bool IsProvincesText { get; set; }
  public string ProvincesLabel { get; set; } = null!;
  public string PostalCodeLabel { get; set; } = null!;
  public bool IsPostalCodeRequired { get; set; }

  public List<Province> Provinces { get; set; } = new List<Province>();

  public override string ToString()
  {
    return $"{nameof(Country)}({Title}, {Id})";
  }
}