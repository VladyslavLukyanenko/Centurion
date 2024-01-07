using AutoMapper;
using Centurion.Accounts.App.Security.Model;
using Centurion.Accounts.Core.Security;

namespace Centurion.Accounts.Infra.Security;

public class SecurityMappingProfile : Profile
{
  public SecurityMappingProfile()
  {
    CreateMap<MemberRole, MemberRoleData>()
      .ForMember(_ => _.Currency, _ => _.MapFrom(o => o.Currency == null ? (int?) null : o.Currency.Id))
      .ForMember(_ => _.PayoutFrequency,
        _ => _.MapFrom(o => o.PayoutFrequency == null ? (int?) null : o.PayoutFrequency.Id));
  }
}