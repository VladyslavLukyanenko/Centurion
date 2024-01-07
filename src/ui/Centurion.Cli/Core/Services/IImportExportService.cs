using CSharpFunctionalExtensions;

namespace Centurion.Cli.Core.Services;

public interface IImportExportService
{
  Task<Result> ExportAsCsvAsync(Stream output, CancellationToken ct);
  Task<Result> ExportAsJsonAsync(Stream output, CancellationToken ct);
  Task<bool> ImportFromJsonAsync(Stream input, CancellationToken ct);
  Task<bool> ImportFromCsvAsync(Stream input, CancellationToken ct);
}