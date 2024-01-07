using Centurion.Cli.Core.Domain;
using Centurion.Cli.Core.Domain.Accounts;
using Centurion.Cli.Core.Domain.Profiles;
using Centurion.Cli.Core.Domain.Tasks;
using Centurion.Cli.Core.Services.Profiles;
using Centurion.Cli.Core.Services.Proxies;
using Centurion.Cli.Core.ViewModels.Home;
using Centurion.Contracts;
using Centurion.Contracts.Analytics;
using Centurion.Contracts.TaskManager;
using DynamicData;
using Google.Protobuf;
using NodaTime.Extensions;
using NodaTime.Text;
using ReactiveUI;
using AutoMapperProfile = AutoMapper.Profile;
using ProtoDuration = Google.Protobuf.WellKnownTypes.Duration;
using ProtoTimestamp = Google.Protobuf.WellKnownTypes.Timestamp;

namespace Centurion.Cli.Core;

public class AutoMapperGenericProfile : AutoMapperProfile
{
  public AutoMapperGenericProfile()
  {
    CreateMap<ProfileModel, CsvProfileData>()
      .ForMember(_ => _.GroupName, _ => _.Ignore())
      .ReverseMap()
      .ForMember(_ => _.Id, _ => _.Ignore())
      .ForMember(_ => _.Changed, _ => _.Ignore())
      .ForMember(_ => _.Changing, _ => _.Ignore())
      .ForMember(_ => _.ThrownExceptions, _ => _.Ignore());

    CreateMap<Proxy, CsvProxyData>()
      .ForMember(_ => _.GroupName, _ => _.Ignore())
      .ReverseMap()
      .ForMember(_ => _.Id, _ => _.Ignore())
      .ForMember(_ => _.IsAvailable, _ => _.Ignore());

    CreateMap<Account, AccountData>()
      .ForMember(_ => _.UserName, _ => _.MapFrom(q => q.Email))
      .ReverseMap()
      .ForMember(_ => _.Email, _ => _.MapFrom(q => q.UserName))
      .ForMember(_ => _.AccessToken, _ => _.Ignore())
      .ForMember(_ => _.Id, _ => _.Ignore())
      .ForMember(_ => _.DomainEvents, _ => _.Ignore());

    CreateMap<SessionModel, SessionModel>();
    CreateMap<SessionModel, SessionData>()
      .IncludeBase<ReactiveObject, IMessage>()
      .AddTransform<string?>(v => string.IsNullOrEmpty(v) ? null : v)
      .AfterMap((model, data) =>
      {
        if (model.Cookies.Any())
        {
          data.Cookies.AddRange(model.Cookies);
        }

        if (model.Extra.Any())
        {
          data.Extra.Add(model.Extra);
        }
      })
      .IgnoreAllPropertiesWithAnInaccessibleSetter()
      .ReverseMap()
      .IncludeBase<IMessage, ReactiveObject>()
      .AddTransform<string?>(v => v ?? "");

    CreateMap<CheckoutTaskModel, CheckoutTaskModel>()
      .ForMember(_ => _.ThrownExceptions, _ => _.Ignore())
      .ForMember(_ => _.Changed, _ => _.Ignore())
      .ForMember(_ => _.Changing, _ => _.Ignore())
      .ForMember(_ => _.Id, _ => _.Ignore())
      .IgnoreAllPropertiesWithAnInaccessibleSetter();

    CreateMap<IMessage, ReactiveObject>()
      .ForMember(_ => _.Changed, _ => _.Ignore())
      .ForMember(_ => _.Changing, _ => _.Ignore())
      .ForMember(_ => _.ThrownExceptions, _ => _.Ignore())
      .AddTransform<string?>(v => string.IsNullOrEmpty(v) ? null : v)
      .IgnoreAllPropertiesWithAnInaccessibleSetter()
      .ReverseMap()
      .AddTransform<string?>(v => v ?? "")
      .ForMember(_ => _.Descriptor, _ => _.Ignore())
      .IgnoreAllPropertiesWithAnInaccessibleSetter();

    CreateMap<CheckoutTaskGroupData, CheckoutTaskGroupModel>()
      .IncludeBase<IMessage, ReactiveObject>()
      .ForMember(_ => _.UpdatedAt, _ => _.MapFrom(q => q.UpdatedAt.ToDateTimeOffset().ToInstant()))
      .ForMember(_ => _.CreatedAt, _ => _.MapFrom(q => q.CreatedAt.ToDateTimeOffset().ToInstant()))
      .ForMember(_ => _.Tasks, _ => _.Ignore())
      .AfterMap((data, model, ctx) => model.Tasks.AddOrUpdate(data.Tasks.Select(ctx.Mapper.Map<CheckoutTaskModel>)))
      .ReverseMap()
      .IncludeBase<ReactiveObject, IMessage>()
      .ForMember(_ => _.Tasks, _ => _.Ignore())
      .ForMember(_ => _.UpdatedAt,
        _ => _.MapFrom(q => ProtoTimestamp.FromDateTimeOffset(q.UpdatedAt.ToDateTimeOffset())))
      .ForMember(_ => _.CreatedAt,
        _ => _.MapFrom(q => ProtoTimestamp.FromDateTimeOffset(q.CreatedAt.ToDateTimeOffset())))
      .AfterMap((model, data, ctx) => data.Tasks.Add(model.Tasks.Items.Select(ctx.Mapper.Map<CheckoutTaskData>)));

    CreateMap<CheckoutTaskData, CheckoutTaskModel>()
      .IncludeBase<IMessage, ReactiveObject>()
      .ForMember(_ => _.UpdatedAt, _ => _.MapFrom(q => q.UpdatedAt.ToDateTimeOffset().ToInstant()))
      .ForMember(_ => _.CreatedAt, _ => _.MapFrom(q => q.CreatedAt.ToDateTimeOffset().ToInstant()))
      .ForMember(_ => _.Config, _ => _.MapFrom(q => q.Config.ToByteArray()))
      .ForMember(_ => _.CheckoutProxyPoolId,
        _ => _.MapFrom(q => q.HasCheckoutProxyPoolId ? q.CheckoutProxyPoolId : null))
      .ForMember(_ => _.MonitorProxyPoolId,
        _ => _.MapFrom(q => q.HasMonitorProxyPoolId ? q.MonitorProxyPoolId : null))
      .ForMember(_ => _.ProductPicture, _ => _.Ignore())
      .ReverseMap()
      .IncludeBase<ReactiveObject, IMessage>()
      .ForMember(_ => _.UpdatedAt,
        _ => _.MapFrom(q => ProtoTimestamp.FromDateTimeOffset(q.UpdatedAt.ToDateTimeOffset())))
      .ForMember(_ => _.CreatedAt,
        _ => _.MapFrom(q => ProtoTimestamp.FromDateTimeOffset(q.CreatedAt.ToDateTimeOffset())))
      .ForMember(_ => _.Config, _ => _.MapFrom(q => ByteString.CopyFrom(q.Config)))
      .ForMember(_ => _.CheckoutProxyPoolId, _ => _.Condition(t => t.CheckoutProxyPoolId.HasValue))
      .ForMember(_ => _.MonitorProxyPoolId, _ => _.Condition(t => t.MonitorProxyPoolId.HasValue));

    CreateMap<PresetData, CheckoutTaskModel>()
      .ForMember(_ => _.Config, _ => _.MapFrom(q => q.Config.ToByteArray()))
      .ForMember(_ => _.Id, _ => _.Ignore())
      .ForMember(_ => _.CreatedAt, _ => _.Ignore())
      .ForMember(_ => _.UpdatedAt, _ => _.Ignore())
      .ForMember(_ => _.ProfileIds, _ => _.Ignore())
      .ForMember(_ => _.CheckoutProxyPoolId, _ => _.Ignore())
      .ForMember(_ => _.MonitorProxyPoolId, _ => _.Ignore());

    CreateMap<ProfileData, ProfileModel>()
      .IncludeBase<IMessage, ReactiveObject>()
      .ReverseMap()
      .IncludeBase<ReactiveObject, IMessage>()
      .AfterMap((model, data, _) =>
      {
        if (model.BillingAsShipping)
        {
          data.ShippingAddress = null;
        }
      });

    CreateMap<BillingData, BillingModel>()
      .IncludeBase<IMessage, ReactiveObject>()
      .ReverseMap()
      .IncludeBase<ReactiveObject, IMessage>()
      /*.ForMember(_ => _.HolderName, _ => _.MapFrom(q => q.HolderName ?? ""))*/;

    CreateMap<AddressData, AddressModel>()
      .IncludeBase<IMessage, ReactiveObject>()
      .ReverseMap()
      .IncludeBase<ReactiveObject, IMessage>()
      /*.ForMember(_ => _.ProvinceCode, _ => _.MapFrom(q => q.ProvinceCode ?? ""))*/;

    CreateMap<ProxyGroup, ProxyPoolData>()
      .ReverseMap();

    CreateMap<Proxy, ProxyData>()
      .ForMember(_ => _.Value, _ => _.MapFrom(p => p.ToUri().ToString()))
      .ReverseMap()
      .ConstructUsing(d => Proxy.Parse(d.Value))
      .ForAllOtherMembers(_ => _.Ignore());

    CreateMap<CheckoutEntry, CheckoutEntryData>()
      .ForMember(_ => _.Date,
        _ => _.MapFrom(q => LocalDatePattern.Iso.Parse(q.Date).Value.AtMidnight().ToDateTimeUnspecified()))
      .ForMember(_ => _.TotalPrice, _ => _.MapFrom(q => (decimal)q.TotalPrice));
  }
}