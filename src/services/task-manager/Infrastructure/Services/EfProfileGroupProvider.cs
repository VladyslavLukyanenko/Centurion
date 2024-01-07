// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Threading;
// using System.Threading.Tasks;
// using AutoMapper;
// using Centurion.Contracts;
// using Centurion.TaskManager.Application.Services;
// using Centurion.TaskManager.Core;
// using Microsoft.EntityFrameworkCore;
//
// namespace Centurion.TaskManager.Infrastructure.Services
// {
//   public class EfProfileGroupProvider : IProfileGroupProvider
//   {
//     private readonly DbContext _context;
//     private readonly IMapper _mapper;
//
//     public EfProfileGroupProvider(DbContext context, IMapper mapper)
//     {
//       _context = context;
//       _mapper = mapper;
//     }
//
//     public async ValueTask<IList<ProfileGroupData>> GetListAsync(string userId, CancellationToken ct = default)
//     {
//       var entities = await _context.Set<ProfileGroup>().AsNoTracking().Where(_ => _.UserId == userId).ToArrayAsync(ct);
//       return _mapper.Map<IList<ProfileGroupData>>(entities);
//     }
//
//     public async ValueTask<ProfileGroupData> GetByIdAsync(Guid groupId, string userId, CancellationToken ct = default)
//     {
//       var entity = await _context.Set<ProfileGroup>()
//         .AsNoTracking()
//         .Where(_ => _.Id == groupId && _.UserId == userId)
//         .FirstOrDefaultAsync(ct);
//
//       return _mapper.Map<ProfileGroupData>(entity);
//     }
//   }
// }