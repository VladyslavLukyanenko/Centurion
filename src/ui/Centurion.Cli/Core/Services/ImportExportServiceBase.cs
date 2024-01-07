using System.Globalization;
using Centurion.SeedWork.Primitives;
using CSharpFunctionalExtensions;
using CsvHelper;
using Newtonsoft.Json;

namespace Centurion.Cli.Core.Services;

public abstract class ImportExportServiceBase<T, TKey> : IImportExportService
  where T : IEntity<TKey>
  where TKey : IEquatable<TKey>
{
  protected readonly IRepository<T, TKey> Repository;

  protected ImportExportServiceBase(IRepository<T, TKey> repository)
  {
    Repository = repository;
  }

  public async Task<Result> ExportAsJsonAsync(Stream output, CancellationToken ct)
  {
    return await Repository.GetAllAsync(ct).AsTask()
      .OnSuccessTry(async lists =>
      {
        var json = JsonConvert.SerializeObject(lists);
        await using var writer = new StreamWriter(output);
        await writer.WriteAsync(json);
      });
  }

  public async Task<bool> ImportFromJsonAsync(Stream input, CancellationToken ct)
  {
    using var reader = new StreamReader(input);
    var json = await reader.ReadToEndAsync();
    var lists = JsonConvert.DeserializeObject<List<T>>(json);
    if (lists != null)
    {
      await Repository.SaveAsync(lists, ct);
    }

    return true;
  }

  public abstract Task<Result> ExportAsCsvAsync(Stream output, CancellationToken ct);
  public abstract Task<bool> ImportFromCsvAsync(Stream input, CancellationToken ct);

  protected async Task<IList<TData>> ReadRecordsFromCsvAsync<TData>(Stream stream, CancellationToken ct = default)
  {
    using var reader = new StreamReader(stream);
    using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
    return await csv.GetRecordsAsync<TData>(ct).ToListAsync(ct);
  }

  protected async Task WriteRecordsToCsvAsync<TData>(Stream output, IEnumerable<TData> data)
  {
    await using var writer = new StreamWriter(output);
    await using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
    await csv.WriteRecordsAsync(data);
  }
}