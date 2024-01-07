using AutoMapper;
using Centurion.Cli.Core.Domain.Accounts;
using CSharpFunctionalExtensions;

namespace Centurion.Cli.Core.Services.Accounts;

public class AccountImportExportService : ImportExportServiceBase<Account, Guid>, IAccountImportExportService
{
  private readonly IMapper _mapper;

  public AccountImportExportService(IRepository<Account, Guid> repository, IMapper mapper)
    : base(repository)
  {
    _mapper = mapper;
  }

  public override async Task<Result> ExportAsCsvAsync(Stream output, CancellationToken ct)
  {
    return await Repository.GetAllAsync(ct).AsTask()
      .OnSuccessTry(async accounts =>
      {
        var records = new List<CsvAccountData>(accounts.Count);
        var mapped = _mapper.Map<List<CsvAccountData>>(accounts);
        // foreach (var account in mapped)
        // {
        //   account.GroupName = accountGroup.Name;
        // }

        records.AddRange(mapped);

        await WriteRecordsToCsvAsync(output, records);
      });
  }

  public override async Task<bool> ImportFromCsvAsync(Stream input, CancellationToken ct)
  {
    IList<CsvAccountData> data = await ReadRecordsFromCsvAsync<CsvAccountData>(input, ct);
    var accounts = _mapper.Map<IList<Account>>(data);

    await Repository.SaveAsync(accounts, ct);
    return true;
  }
}