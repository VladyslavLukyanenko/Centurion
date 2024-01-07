using AutoMapper;
using Centurion.Accounts.App.Audit.Data;
using Centurion.Accounts.App.Identity.Model;
using Centurion.Accounts.Core.Audit;

namespace Centurion.Accounts.Infra.Audit;

public class AuditMapperProfile : Profile
{
  public AuditMapperProfile()
  {
    CreateMap<ChangeSet, ChangeSetData>()
      .ForMember(_ => _.UpdatedBy, _ => _.MapFrom(o => new UserRef {Id = o.UpdatedBy}))
      //.ForMember(_ => _.Facility, _ => _.MapFrom(o => new FacilityRef {Id = o.FacilityId.GetValueOrDefault()}))
      ;

    CreateMap<ChangeSetEntry, ChangeSetEntryRefData>();
  }
}