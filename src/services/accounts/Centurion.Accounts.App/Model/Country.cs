#pragma warning disable 8618

namespace Centurion.Accounts.App.Model;

public class Country
{
  public string Id { get; set; }
  public string Title { get; set; }
  public bool IsProvincesRequired { get; set; }
  public bool IsProvincesList { get; set; }
  public bool IsProvincesText { get; set; }
  public string ProvincesLabel { get; set; }
  public string PostalCodeLabel { get; set; }
  public bool IsPostalCodeRequired { get; set; }

  public List<Province> Provinces { get; set; } = new List<Province>();

  public override string ToString()
  {
    return $"{nameof(Country)}({Title}, {Id})";
  }
}