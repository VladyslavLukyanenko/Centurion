using Centurion.Accounts.Core.Forms;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Centurion.Accounts.Infra.Forms.EfMappings;

public class FormComponentMappingConfig : FormComponentsMappingConfigBase<FormComponent>
{
}

public class FormSectionMappingConfig : EfFormsMappingConfigBase<FormSection>
{
  public FormSectionMappingConfig()
  {
    MappedToSeparateTable = false;
  }
}

public class FormFieldMappingConfig : EfFormsMappingConfigBase<FormField>
{
  public FormFieldMappingConfig()
  {
    MappedToSeparateTable = false;
  }

  public override void Configure(EntityTypeBuilder<FormField> builder)
  {
    builder.HasOne<FormSection>()
      .WithMany()
      .HasForeignKey(_ => _.SectionId);

    base.Configure(builder);
  }
}

public class LinearScaleFieldMappingConfig : EfFormsMappingConfigBase<LinearScaleField>
{
  public LinearScaleFieldMappingConfig()
  {
    MappedToSeparateTable = false;
  }

  public override void Configure(EntityTypeBuilder<LinearScaleField> builder)
  {
    builder.Property(_ => _.MaxLabel).IsRequired();
    builder.Property(_ => _.MinLabel).IsRequired();
    base.Configure(builder);
  }
}

public class MultiChoiceFieldMappingConfig : EfFormsMappingConfigBase<MultiChoiceField>
{
  public MultiChoiceFieldMappingConfig()
  {
    MappedToSeparateTable = false;
  }

  public override void Configure(EntityTypeBuilder<MultiChoiceField> builder)
  {
    builder.Property(_ => _.Options)
      .HasColumnType("jsonb")
      .HasConversion(o => ToJson(o), json => FromJson<IList<RichFormOptionValue>>(json)!);
    base.Configure(builder);
  }
}

public class MultiChoiceGridFieldMappingConfig : EfFormsMappingConfigBase<MultiChoiceGridField>
{
  public MultiChoiceGridFieldMappingConfig()
  {
    MappedToSeparateTable = false;
  }

  public override void Configure(EntityTypeBuilder<MultiChoiceGridField> builder)
  {
    builder.Property(_ => _.Rows)
      .HasColumnType("jsonb")
      .HasConversion(o => ToJson(o), json => FromJson<IList<FormOptionValue>>(json)!);

    builder.Property(_ => _.Columns)
      .HasColumnType("jsonb")
      .HasConversion(o => ToJson(o), json => FromJson<IList<FormOptionValue>>(json)!);

    base.Configure(builder);
  }
}

public class TextBlockFieldMappingConfig : EfFormsMappingConfigBase<TextBlockField>
{
  public TextBlockFieldMappingConfig()
  {
    MappedToSeparateTable = false;
  }
}

public class ParagraphFieldMappingConfig : EfFormsMappingConfigBase<ParagraphField>
{
  public ParagraphFieldMappingConfig()
  {
    MappedToSeparateTable = false;
  }
}

public class CheckBoxesFieldMappingConfig : EfFormsMappingConfigBase<CheckBoxesField>
{
  public CheckBoxesFieldMappingConfig()
  {
    MappedToSeparateTable = false;
  }

  public override void Configure(EntityTypeBuilder<CheckBoxesField> builder)
  {
    builder.Property(_ => _.Options)
      .HasColumnType("jsonb")
      .HasConversion(o => ToJson(o), json => FromJson<IList<RichFormOptionValue>>(json)!);
    base.Configure(builder);
  }
}

public class CheckBoxesGridFieldMappingConfig : EfFormsMappingConfigBase<CheckBoxesGridField>
{
  public CheckBoxesGridFieldMappingConfig()
  {
    MappedToSeparateTable = false;
  }

  public override void Configure(EntityTypeBuilder<CheckBoxesGridField> builder)
  {
    builder.Property(_ => _.Rows)
      .HasColumnType("jsonb")
      .HasConversion(o => ToJson(o), json => FromJson<IList<FormOptionValue>>(json)!);

    builder.Property(_ => _.Columns)
      .HasColumnType("jsonb")
      .HasConversion(o => ToJson(o), json => FromJson<IList<FormOptionValue>>(json)!);

    base.Configure(builder);
  }
}

public class DropDownFieldMappingConfig : EfFormsMappingConfigBase<DropDownField>
{
  public DropDownFieldMappingConfig()
  {
    MappedToSeparateTable = false;
  }

  public override void Configure(EntityTypeBuilder<DropDownField> builder)
  {
    builder.Property(_ => _.Options)
      .HasColumnType("jsonb")
      .HasConversion(o => ToJson(o), json => FromJson<IList<FormOptionValue>>(json)!);
    base.Configure(builder);
  }
}

public class TextBoxFieldMappingConfig : EfFormsMappingConfigBase<TextBoxField>
{
  public TextBoxFieldMappingConfig()
  {
    MappedToSeparateTable = false;
  }
}

public class TimeFieldMappingConfig : EfFormsMappingConfigBase<TimeField>
{
  public TimeFieldMappingConfig()
  {
    MappedToSeparateTable = false;
  }
}