using System.Reactive.Linq;
using Avalonia.Controls.Mixins;
using Centurion.Cli.Core.Domain.Profiles;
using Centurion.Cli.Core.Services;
using Centurion.Cli.Core.Validators;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Centurion.Cli.Core.ViewModels.Profiles;

public class AddressEditorViewModel : ViewModelBase
{
#if DEBUG
  public AddressEditorViewModel()
  {
  }
#endif
  public AddressEditorViewModel(ICountriesService countriesService, AddressModel address,
    AddressValidator validator)
  {
    Countries = countriesService.Countries;
    var countriesCache = Countries.ToDictionary(_ => _.Id);
    if (!string.IsNullOrEmpty(address.CountryId) && countriesCache.ContainsKey(address.CountryId))
    {
      SelectedCountry = countriesCache[address.CountryId];
    }
    else
    {
      SelectedCountry = Countries[0];
    }

    Address = address;
    this.WhenAnyValue(_ => _.SelectedCountry)
      .Select(_ => _?.Id)
      .Subscribe(countryId =>
      {
        Address.CountryId = countryId!;
        if (!string.IsNullOrEmpty(countryId) && countriesCache.ContainsKey(countryId))
        {
          SelectedState = SelectedCountry?.Provinces.FirstOrDefault(_ => _.Code == address.ProvinceCode);
        }
        else
        {
          SelectedState = SelectedCountry.Provinces.FirstOrDefault();
        }

        IsProvinceListVisible = SelectedCountry?.IsProvincesList ?? false;
        IsProvinceInputVisible = SelectedCountry?.IsProvincesText ?? false;
        IsProvinceLabelVisible = IsProvinceListVisible || IsProvinceInputVisible;
      })
      .DisposeWith(Disposable);

    this.WhenAnyValue(_ => _.SelectedState)
      .CombineLatest(this.WhenAnyValue(_ => _.IsProvinceListVisible), (province, isVisible) => (province, isVisible))
      .Where(_ => _.isVisible)
      .Select(_ => _.province?.Code)
      .Subscribe(code => Address.ProvinceCode = code)
      .DisposeWith(Disposable);

    this.WhenAnyValue(_ => _.SelectedProvinceText)
      .CombineLatest(this.WhenAnyValue(_ => _.IsProvinceInputVisible), (text, isVisible) => (text, isVisible))
      .Where(_ => _.isVisible)
      .Subscribe(code => Address.ProvinceCode = code.text)
      .DisposeWith(Disposable);

    Address.Changed.Throttle(TimeSpan.FromMilliseconds(200))
      .Select(_ => validator.Validate(Address).IsValid)
      .ToPropertyEx(this, _ => _.IsValid)
      .DisposeWith(Disposable);
  }

  public bool IsValid { [ObservableAsProperty] get; }
  [Reactive] public AddressModel Address { get; private set; }

  [Reactive] public bool IsProvinceLabelVisible { get; private set; }
  [Reactive] public bool IsProvinceListVisible { get; private set; }
  [Reactive] public bool IsProvinceInputVisible { get; private set; }

  [Reactive] public string? SelectedProvinceText { get; set; }
  public IReadOnlyList<Country> Countries { get; }
  [Reactive] public Country? SelectedCountry { get; set; }
  [Reactive] public Province? SelectedState { get; set; }
}