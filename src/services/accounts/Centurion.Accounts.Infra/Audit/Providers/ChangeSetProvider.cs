using Microsoft.EntityFrameworkCore;
using Centurion.Accounts.App.Audit.Data;
using Centurion.Accounts.App.Audit.Services;
using Centurion.Accounts.Core.Audit;
using Centurion.Accounts.Core.Collections;
using Centurion.Accounts.Infra.Converters;
using Centurion.Accounts.Infra.Services;

namespace Centurion.Accounts.Infra.Audit.Providers;

public class ChangeSetProvider : DataProvider, IChangeSetProvider
{
  private readonly IChangeSetRepository _changeSetRepository;
  private readonly IChangeSetEntryPayloadMapper _payloadMapper;
  private readonly IDataConverter<ChangeSet, ChangeSetData> _changeSetDataConverter;

  private readonly IQueryable<ChangeSet> _changeSets;

  public ChangeSetProvider(IChangeSetRepository changeSetRepository, IChangeSetEntryPayloadMapper payloadMapper,
    DbContext context, IDataConverter<ChangeSet, ChangeSetData> changeSetDataConverter)
    : base(context)
  {
    _changeSetRepository = changeSetRepository;
    _payloadMapper = payloadMapper;
    _changeSetDataConverter = changeSetDataConverter;
    _changeSets = GetDataSource<ChangeSet>();
  }

  public async Task<IPagedList<ChangeSetData>> GetChangeSetPageAsync(ChangeSetPageRequest pageRequest,
    CancellationToken ct = default)
  {
    var query = _changeSets.Where(_ => _.Timestamp >= pageRequest.From && _.Timestamp <= pageRequest.To);

    if (pageRequest.UserId.HasValue)
    {
      query = query.Where(_ => _.UpdatedBy == pageRequest.UserId);
    }

    if (pageRequest.ChangeType.HasValue)
    {
      query = query.Where(m => m.Entries.Any(_ => _.ChangeType == pageRequest.ChangeType));
    }

    var changeSetPage = await query
      .OrderByDescending(m => m.Timestamp)
      .PaginateAsync(pageRequest, ct);

    return changeSetPage.CopyWith(await _changeSetDataConverter.ConvertAsync(changeSetPage.Content, ct));
  }

  public async Task<ChangesetEntryPayloadData?> GetPayloadAsync(Guid changeSetEntryId,
    CancellationToken ct = default)
  {
    var current = await _changeSetRepository.GetEntryByIdAsync(changeSetEntryId, ct);

    if (current == null)
    {
      return null;
    }

    var currentPayload = await _payloadMapper.MapAsync(current, ct);
    if (currentPayload == null)
    {
      return null;
    }

    IDictionary<string, string?>? previous = null;
    if (current.ChangeType == ChangeType.Modification)
    {
      var prev = await _changeSetRepository.GetPreviousAsync(current, ct);
      previous = await _payloadMapper.MapAsync(prev!, ct);
    }

    return new ChangesetEntryPayloadData
    {
      Previous = previous,
      Current = currentPayload
    };
  }
}