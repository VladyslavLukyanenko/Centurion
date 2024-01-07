using Centurion.CloudManager.Domain;
using Centurion.CloudManager.Domain.Services;
using Centurion.SeedWork.Infra.EfCoreNpgsql.Services;
using Centurion.SeedWork.Primitives;
using Microsoft.EntityFrameworkCore;

namespace Centurion.CloudManager.Infra.Repositories;

public class EfImageInfoRepository : EfCrudRepository<ImageInfo, string>, IImageInfoRepository
{
  public EfImageInfoRepository(DbContext context, IUnitOfWork unitOfWork) : base(context, unitOfWork)
  {
  }

  public async ValueTask<IDictionary<string, ImageInfo>> GetLatestByNames(ISet<string> images,
    CancellationToken ct = default)
  {
    return await DataSource.Where(_ => images.Contains(_.Name))
      .GroupBy(_ => _.Name)
      .Select(_ => _.OrderByDescending(q => q.CreatedAt).First())
      .ToDictionaryAsync(_ => _.Name, ct);
  }
}