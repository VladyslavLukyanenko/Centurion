using AutoMapper;
using Centurion.Contracts;
using Centurion.TaskManager.Application.Services;
using Centurion.TaskManager.Core;
using Microsoft.EntityFrameworkCore;

namespace Centurion.TaskManager.Infrastructure.Services;

public class EfCheckoutTaskGroupProvider : ICheckoutTaskGroupProvider
{
  private readonly DbContext _context;
  private readonly IMapper _mapper;

  public EfCheckoutTaskGroupProvider(DbContext context, IMapper mapper)
  {
    _context = context;
    _mapper = mapper;
  }

  public async ValueTask<IList<CheckoutTaskGroupData>> GetListAsync(string userId, CancellationToken ct = default)
  {
    var tasks = await _context.Set<CheckoutTask>()
      .AsNoTracking()
      .Where(_ => _.UserId == userId)
      .ToArrayAsync(ct);

    var groups = await _context.Set<CheckoutTaskGroup>()
      .AsNoTracking()
      .Where(_ => _.UserId == userId)
      .ToArrayAsync(ct);


    var tasksLookup = tasks.ToLookup(_ => _.GroupId);
    var groupDataList = new List<CheckoutTaskGroupData>(groups.Length);
    foreach (var taskGroup in groups)
    {
      var data = _mapper.Map<CheckoutTaskGroupData>(taskGroup);
      var taskDataList = _mapper.Map<CheckoutTaskData[]>(tasksLookup[taskGroup.Id]);
      data.Tasks.AddRange(taskDataList);

      groupDataList.Add(data);
    }

    return groupDataList;
  }
}