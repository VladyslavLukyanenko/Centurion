using System.Collections.ObjectModel;
using System.Reactive.Linq;
using AutoMapper;
using Avalonia.Controls;
using Avalonia.Controls.Mixins;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Centurion.Cli.Core.Services.Modules.Accessors;
using Centurion.Cli.Core.Services.Sessions;
using Centurion.Contracts;
using DynamicData;
using DynamicData.Binding;
using Type = Centurion.Contracts.Type;

namespace Centurion.Cli.AvaloniaUI.Services.Fields;

public class SessionFieldControlFactory : SingleValueFieldControlFactoryBase<object>
{
  private readonly ReadOnlyObservableCollection<SessionData> _sessions;

  public SessionFieldControlFactory(ISessionRepository sessions, IMapper mapper)
  {
    sessions.Items.Connect()
      .Transform(mapper.Map<SessionData>)
      .Bind(out _sessions)
      .Subscribe();
  }

  public override bool IsSupported(IConfigFieldAccessor field) =>
    field.Descriptor.Type is Type.Message && field.Descriptor.TypeName == "." + nameof(SessionData);

  protected override Control CreateEditorControl(IConfigFieldAccessor<object> field)
  {
    var comboBox = new ComboBox
    {
      PlaceholderText = field.Descriptor.Name,
      Items = _sessions,
      SelectedItem = field.GetValue(),
      ItemTemplate = new FuncDataTemplate<SessionData>((s, _) => new TextBlock
        { [!TextBlock.TextProperty] = new Binding(nameof(s.Name)) })
    };

    comboBox.WhenValueChanged(_ => _.SelectedItem)
      .DistinctUntilChanged()
      .Subscribe(field.SetValue)
      .DisposeWith(field.Disposable);

    return comboBox;
  }
}