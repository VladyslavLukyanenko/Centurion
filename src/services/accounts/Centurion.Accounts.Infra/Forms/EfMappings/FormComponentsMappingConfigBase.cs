using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Centurion.Accounts.Core.Forms;

namespace Centurion.Accounts.Infra.Forms.EfMappings;

public abstract class FormComponentsMappingConfigBase<T> : EfFormsMappingConfigBase<T>
  where T: FormComponent
{
  public override void Configure(EntityTypeBuilder<T> builder)
  {
    builder.HasOne<Form>()
      .WithMany()
      .HasForeignKey(_ => _.FormId);

    builder.OwnsOne(_ => _.Name);
    base.Configure(builder);
  }
}