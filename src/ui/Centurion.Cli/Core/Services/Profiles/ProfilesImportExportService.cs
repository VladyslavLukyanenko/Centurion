using AutoMapper;
using Centurion.Cli.Core.Domain.Profiles;
using CSharpFunctionalExtensions;
using Newtonsoft.Json;

namespace Centurion.Cli.Core.Services.Profiles;

public class ProfilesImportExportService : ImportExportServiceBase<ProfileGroupModel, Guid>,
  IProfilesImportExportService
{
  private readonly IMapper _mapper;

  public ProfilesImportExportService(IProfilesRepository repository, IMapper mapper)
    : base(repository)
  {
    _mapper = mapper;
  }

  public override async Task<Result> ExportAsCsvAsync(Stream output, CancellationToken ct)
  {
    return await Repository.GetAllAsync(ct).AsTask()
      .OnSuccessTry(async profiles =>
      {
        var records = new List<CsvProfileData>(profiles.Sum(_ => _.Profiles.Count));
        foreach (var group in profiles)
        {
          var mapped = _mapper.Map<List<CsvProfileData>>(group.Profiles);
          foreach (var data in mapped)
          {
            data.GroupName = @group.Name;
          }

          records.AddRange(mapped);
        }

        await WriteRecordsToCsvAsync(output, records);
      });
  }

  public override async Task<bool> ImportFromCsvAsync(Stream input, CancellationToken ct)
  {
    IList<CsvProfileData> data = await ReadRecordsFromCsvAsync<CsvProfileData>(input, ct);
    var groups = data.GroupBy(_ => _.GroupName)
      .Select(group =>
      {
        var profiles = _mapper.Map<IList<ProfileModel>>(group);
        var profileGroup = new ProfileGroupModel
        {
          Name = group.Key,
        };

        using var _ = profileGroup.Profiles.SuspendNotifications();
        profileGroup.Profiles.AddRange(profiles);
        return profileGroup;
      });

    await Repository.SaveAsync(groups, ct);
    return true;
  }

  public async Task<bool> ImportFromJsonIntoGroupAsync(FileStream file, ProfileGroupModel selectedGroup,
    CancellationToken ct = default)
  {
    using var reader = new StreamReader(file);
    var json = await reader.ReadToEndAsync();
    var list = JsonConvert.DeserializeObject<List<ProfileModel>>(json);
    if (list is not null)
    {
      using var _ = selectedGroup.Profiles.SuspendNotifications();
      selectedGroup.Profiles.AddRange(list);
      await Repository.SaveSilentlyAsync(selectedGroup, ct);
    }

    return list is not null;
  }
}