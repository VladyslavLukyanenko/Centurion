using System.Collections.ObjectModel;
using System.Reactive.Linq;
using AutoMapper;
using Avalonia.Controls;
using Avalonia.Controls.Mixins;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Centurion.Cli.Core.Services.Accounts;
using Centurion.Cli.Core.Services.Modules.Accessors;
using Centurion.Contracts;
using DynamicData;
using DynamicData.Binding;
using Type = Centurion.Contracts.Type;

namespace Centurion.Cli.AvaloniaUI.Services.Fields;

public class AccountFieldControlFactory : SingleValueFieldControlFactoryBase<object>
{
  private readonly ReadOnlyObservableCollection<AccountData> _accounts;

  public AccountFieldControlFactory(IAccountsRepository accounts, IMapper mapper)
  {
    accounts.Items.Connect()
      .Transform(mapper.Map<AccountData>)
      .Bind(out _accounts)
      .Subscribe();
  }

  public override bool IsSupported(IConfigFieldAccessor field) =>
    field.Descriptor.Type is Type.Message && field.Descriptor.TypeName == "." + nameof(AccountData);

  protected override Control CreateEditorControl(IConfigFieldAccessor<object> field)
  {
    var comboBox = new ComboBox
    {
      PlaceholderText = field.Descriptor.Name,
      Items = _accounts,
      SelectedItem= field.GetValue(),
      ItemTemplate = new FuncDataTemplate<AccountData>((acc, _) => new TextBlock
        { [!TextBlock.TextProperty] = new Binding(nameof(acc.UserName)) })
    };

    comboBox.WhenValueChanged(_ => _.SelectedItem)
      .DistinctUntilChanged()
      .Subscribe(field.SetValue)
      .DisposeWith(field.Disposable);

    return comboBox;
  }
}