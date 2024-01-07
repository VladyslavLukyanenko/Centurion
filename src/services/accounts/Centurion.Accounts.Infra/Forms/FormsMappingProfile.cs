using AutoMapper;
using Centurion.Accounts.App.Forms.Model;
using Centurion.Accounts.Core.Forms;

namespace Centurion.Accounts.Infra.Forms;

public class FormsMappingProfile : Profile
{
  public FormsMappingProfile()
  {
    CreateMap<Form, FormData>()
      .ForMember(_ => _.Sections, _ => _.MapFrom(o => new List<FormSectionData>()));

    CreateMap<FormSection, FormSectionData>()
      .ForMember(_ => _.Fields, _ => _.MapFrom(o => new List<FormFieldData>()));

    CreateMap<FormField, FormFieldData>()
      .ForMember(_ => _.Type, x => x.MapFrom(_ => _.GetType().Name))
      .ForMember(_ => _.Value, x => x.Ignore());

    // Date, Time, TextBlock
    CreateMap<DateField, FormFieldData>().IncludeBase<FormField, FormFieldData>();
    CreateMap<TimeField, FormFieldData>().IncludeBase<FormField, FormFieldData>();
    CreateMap<TextBlockField, FormFieldData>().IncludeBase<FormField, FormFieldData>();

    // CheckBox, DropDown, MultiChoice
    CreateMap<CheckBoxesField, SelectableFieldData>().IncludeBase<FormField, FormFieldData>();
    CreateMap<MultiChoiceField, SelectableFieldData>().IncludeBase<FormField, FormFieldData>();
    CreateMap<DropDownField, SelectableFieldData>()
      .IncludeBase<FormField, FormFieldData>()
      .ForMember(_ => _.Options, x => x.MapFrom(_ => _.Options.Select(it => new RichFormOptionValue
      {
        Id = it.Id,
        Order = it.Order,
        Text = new RichFormFieldTitle
        {
          Value = it.Text
        }
      })));
      
    // TextBox, Paragraph
    CreateMap<TextBoxField, TextFieldData>().IncludeBase<FormField, FormFieldData>();
    CreateMap<ParagraphField, TextFieldData>().IncludeBase<FormField, FormFieldData>();
  }
}