using Centurion.Accounts.Core.Forms;

namespace Centurion.Accounts.App.Forms.Model;

public class FormData
{
  public long Id { get; set; }
  public Guid DashboardId { get; set; }
  public FormSettings Settings { get; set; } = null!;
  public FormThemeSettings Theme { get; set; } = null!;
  public IList<FormSectionData> Sections { get; set; } = new List<FormSectionData>();
}

public class FormSectionData
{
  public long Id { get; set; }
  public RichFormFieldTitle Name { get; set; } = null!;
  public uint Order { get; set; }
  public string? Description { get; set; }
  public IList<FormFieldData> Fields { get; set; } = new List<FormFieldData>();
}

// Date, Time, TextBlock
public class FormFieldData
{
  public long Id { get; set; }
  public RichFormFieldTitle? Name { get; set; }
  public uint Order { get; set; }
  public string? Description { get; set; }
  public string Type { get; set; } = null!;
  public string? Value { get; set; }
}

// CheckBox, DropDown, MultiChoice
public class SelectableFieldData : FormFieldData
{
  public IList<RichFormOptionValue> Options { get; set; } = new List<RichFormOptionValue>();
}

// CheckBoxGrid, MultiChoiceGrid
public class GridFieldData : FormFieldData
{
  public IList<FormOptionValue> Rows { get; set; } = new List<FormOptionValue>();
  public IList<FormOptionValue> Columns { get; set; } = new List<FormOptionValue>();
}

// TextBox, Paragraph
public class TextFieldData : FormFieldData
{
  public string? Placeholder { get; set; }
}