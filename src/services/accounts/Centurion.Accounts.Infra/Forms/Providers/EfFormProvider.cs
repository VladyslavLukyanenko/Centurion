using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Centurion.Accounts.App.Forms.Model;
using Centurion.Accounts.App.Forms.Services;
using Centurion.Accounts.Core.Forms;
using Centurion.Accounts.Core.Products;
using Centurion.Accounts.Infra.Services;

namespace Centurion.Accounts.Infra.Forms.Providers;

public class EfFormProvider : DataProvider, IFormProvider
{
  private readonly IQueryable<Form> _aliveForms;
  private readonly IMapper _mapper;

  public EfFormProvider(DbContext context, IMapper mapper)
    : base(context)
  {
    _mapper = mapper;
    _aliveForms = GetAliveDataSource<Form>();
  }

  public async ValueTask<FormData?> GetUserFormAsync(long formId, long userId, CancellationToken ct = default)
  {
    var componentsSource = GetAliveDataSource<FormComponent>();
    var responsesSource = GetAliveDataSource<FormResponse>();
    var dashboardsSource = GetAliveDataSource<Dashboard>();

    var form = await _aliveForms
      .Where(_ => _.Settings.AllowAccessToResults
                  || dashboardsSource.Any(d => d.OwnerId == userId && d.Id == _.DashboardId))
      .FirstOrDefaultAsync(_ => _.Id == formId, ct);
    if (form == null)
    {
      return null;
    }

    var formData = _mapper.Map<FormData>(form);
    var values = await responsesSource.Where(_ => _.FormId == formId && _.RespondedBy == userId)
      .SelectMany(_ => _.FieldValues)
      .ToDictionaryAsync(_ => _.FieldId, _ => _.Value, ct);

    var components = await componentsSource.Where(_ => _.FormId == formId)
      .OrderBy(_ => _.Order)
      .ToArrayAsync(ct);

    var sections = _mapper.Map<IList<FormSectionData>>(components.OfType<FormSection>());
    foreach (var field in components.OfType<FormField>())
    {
      var section = sections.First(_ => _.Id == field.SectionId);
      var fieldData = _mapper.Map<FormFieldData>(field);
      section.Fields.Add(fieldData);
      if (values.TryGetValue(field.Id, out var value))
      {
        fieldData.Value = value;
      }
    }

    foreach (var section in sections)
    {
      formData.Sections.Add(section);
    }

    return formData;
  }
}