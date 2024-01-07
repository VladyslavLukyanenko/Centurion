using Centurion.Accounts.Core.Products;
using CSharpFunctionalExtensions;

namespace Centurion.Accounts.Core.Forms;

public class FormResponse : DashboardBoundEntity
{
  private List<FormFieldValue> _fieldValues = new();
  public long FormId { get; private set; }
  public long RespondedBy { get; private set; }

  private FormResponse()
  {
  }

  private FormResponse(long formId, long respondedBy, IEnumerable<FormFieldValue> values)
  {
    FormId = formId;
    RespondedBy = respondedBy;
    _fieldValues.AddRange(values);
  }

  public IReadOnlyList<FormFieldValue> FieldValues => _fieldValues.AsReadOnly();

  public static Result<FormResponse> ValidateAndCreate(long formId, long respondedBy,
    IEnumerable<FormFieldValue> values,
    IEnumerable<FormField> fields)
  {
    var valuesDict = values.ToDictionary(_ => _.FieldId);
    var validationResult = Result.Success();
    foreach (var field in fields)
    {
      if (field.FormId != formId)
      {
        validationResult =
          Result.Combine(validationResult, Result.Failure($"Form Id of field '{field.Id}' is invalid"));
      }

      valuesDict.TryGetValue(field.Id, out var value);
      if (!field.IsValid(value))
      {
        validationResult =
          Result.Combine(validationResult, Result.Failure($"Value of field '{field.Id}' is invalid"));
      }
    }

    if (validationResult.IsFailure)
    {
      return validationResult.ConvertFailure<FormResponse>();
    }

    return new FormResponse(formId, respondedBy, valuesDict.Values);
  }
}