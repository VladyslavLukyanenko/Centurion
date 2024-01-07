using Centurion.Accounts.App.Identity.Model;
using Centurion.Accounts.Core.Identity;
using Profile = AutoMapper.Profile;

namespace Centurion.Accounts.Infra.Identity;

public class UserMapperProfile : Profile
{
  public UserMapperProfile()
  {
    CreateMap<User, UserData>()
      .ForMember(_ => _.Email, _ => _.MapFrom(o => o.Email.Value))
      .ForMember(_ => _.IsEmailConfirmed, _ => _.MapFrom(o => o.Email.IsConfirmed))
      .ForMember(_ => _.IsLockedOut, _ => _.MapFrom(o => o.IsLockedOut))
      .ForMember(_ => _.Roles, _ => _.Ignore());
  }
}