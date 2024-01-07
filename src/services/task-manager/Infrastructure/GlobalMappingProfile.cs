using AutoMapper;
using Centurion.Contracts;
using Centurion.Contracts.Analytics;
using Centurion.Contracts.Checkout;
using Centurion.Contracts.Checkout.Integration;
using Centurion.Contracts.TaskManager;
using Centurion.SeedWork.Collections;
using Centurion.SeedWork.Infra.EfCoreNpgsql;
using Centurion.TaskManager.Contracts;
using Centurion.TaskManager.Core;
using Centurion.TaskManager.Core.Events;
using Centurion.TaskManager.Core.Presets;
using Google.Protobuf;
using NodaTime.Extensions;
using CheckoutTask = Centurion.TaskManager.Core.CheckoutTask;
// using StackExchange.Redis.Extensions.Core.Configuration;
using ProtoDuration = Google.Protobuf.WellKnownTypes.Duration;
using ProtoTimestamp = Google.Protobuf.WellKnownTypes.Timestamp;

namespace Centurion.TaskManager.Infrastructure;

public class GlobalMappingProfile : Profile
{
  public GlobalMappingProfile()
  {
    CreateMap<CheckoutTask, InitializedCheckoutTaskData>()
      .ForMember(_ => _.Config, _ => _.MapFrom(q => ByteString.CopyFrom(q.Config)))
      .ForMember(_ => _.ProfileList, _ => _.Ignore())
      .ForMember(_ => _.Product, _ => _.Ignore())
      .ForMember(_ => _.ProxyPool, _ => _.Ignore())
      .IgnoreAllPropertiesWithAnInaccessibleSetter();

    CreateMap<CheckoutTaskState, ICheckoutTaskDataState>()
      .AddTransform<string?>(v => v ?? "")
      .ForMember(_ => _.Config, _ => _.MapFrom(q => ByteString.CopyFrom(q.Config)))
      .ReverseMap()
      .AddTransform<string?>(v => v.ValueOrNull())
      .ForMember(_ => _.Config, _ => _.MapFrom(q => q.Config.ToByteArray()))
      .IgnoreAllPropertiesWithAnInaccessibleSetter();

    CreateMap<CheckoutTaskGroup, CheckoutTaskGroupData>()
      .AddTransform<string?>(v => v ?? "")
      .ForMember(_ => _.UpdatedAt, _ => _.MapFrom(q => ProtoTimestamp.FromDateTimeOffset(q.UpdatedAt.ToDateTimeOffset())))
      .ForMember(_ => _.CreatedAt, _ => _.MapFrom(q => ProtoTimestamp.FromDateTimeOffset(q.CreatedAt.ToDateTimeOffset())))
      .ForMember(_ => _.Tasks, _ => _.Ignore())
      .ReverseMap()
      .AddTransform<string?>(v => v.ValueOrNull())
      .ForMember(_ => _.UpdatedAt, _ => _.MapFrom(q => q.UpdatedAt.ToDateTimeOffset().ToInstant()))
      .ForMember(_ => _.CreatedAt, _ => _.MapFrom(q => q.CreatedAt.ToDateTimeOffset().ToInstant()))
      .IgnoreAllPropertiesWithAnInaccessibleSetter();

    CreateMap<CheckoutTask, CheckoutTaskData>()
      .IncludeBase<CheckoutTaskState, ICheckoutTaskDataState>()
      .AddTransform<string?>(v => v ?? "")
      .ForMember(_ => _.UpdatedAt, _ => _.MapFrom(q => ProtoTimestamp.FromDateTimeOffset(q.UpdatedAt.ToDateTimeOffset())))
      .ForMember(_ => _.CreatedAt, _ => _.MapFrom(q => ProtoTimestamp.FromDateTimeOffset(q.CreatedAt.ToDateTimeOffset())))
      .ForMember(_ => _.CheckoutProxyPoolId, _ => _.Condition(t => t.CheckoutProxyPoolId.HasValue))
      .ForMember(_ => _.MonitorProxyPoolId, _ => _.Condition(t => t.MonitorProxyPoolId.HasValue))
      .ReverseMap()
      .IncludeBase<ICheckoutTaskDataState, CheckoutTaskState>()
      .AddTransform<string?>(v => v.ValueOrNull())
      .ForMember(_ => _.GroupId, _ => _.Ignore())
      .ForMember(_ => _.UpdatedAt, _ => _.MapFrom(q => q.UpdatedAt.ToDateTimeOffset().ToInstant()))
      .ForMember(_ => _.CreatedAt, _ => _.MapFrom(q => q.CreatedAt.ToDateTimeOffset().ToInstant()))
      .ForMember(_ => _.CheckoutProxyPoolId,
        _ => _.MapFrom(q => q.HasCheckoutProxyPoolId ? q.CheckoutProxyPoolId : null))
      .ForMember(_ => _.MonitorProxyPoolId,
        _ => _.MapFrom(q => q.HasMonitorProxyPoolId ? q.MonitorProxyPoolId : null))
      .IgnoreAllPropertiesWithAnInaccessibleSetter();

    CreateMap<Preset, PresetData>()
      .IncludeBase<CheckoutTaskState, ICheckoutTaskDataState>()
      .AddTransform<string?>(v => v ?? "")
      .ForMember(_ => _.ExpectedAt, _ => _.MapFrom(q => ProtoTimestamp.FromDateTimeOffset(q.ExpectedAt.ToDateTimeOffset())))
      .ForMember(_ => _.UpdatedAt, _ => _.MapFrom(q => ProtoTimestamp.FromDateTimeOffset(q.UpdatedAt.ToDateTimeOffset())))
      .ForMember(_ => _.CreatedAt, _ => _.MapFrom(q => ProtoTimestamp.FromDateTimeOffset(q.CreatedAt.ToDateTimeOffset())))
      .ReverseMap()
      .IncludeBase<ICheckoutTaskDataState, CheckoutTaskState>()
      .AddTransform<string?>(v => v.ValueOrNull())
      .ForMember(_ => _.ExpectedAt, _ => _.MapFrom(q => q.ExpectedAt.ToDateTimeOffset().ToInstant()))
      .ForMember(_ => _.UpdatedAt, _ => _.MapFrom(q => q.UpdatedAt.ToDateTimeOffset().ToInstant()))
      .ForMember(_ => _.CreatedAt, _ => _.MapFrom(q => q.CreatedAt.ToDateTimeOffset().ToInstant()))
      .IgnoreAllPropertiesWithAnInaccessibleSetter();

    CreateMap<Product, ProductData>()
      .AddTransform<string?>(v => v ?? "")
      .ReverseMap()
      .AddTransform<string?>(v => v.ValueOrNull())
      .IgnoreAllPropertiesWithAnInaccessibleSetter();

    CreateMap<ProductCheckedOut, ProductCheckedOutEvent>()
      .ForMember(_ => _.Delay, _ => _.MapFrom(q => q.Delay.ToTimeSpan().ToDuration()))
      .ForMember(_ => _.Duration, _ => _.MapFrom(q => q.Duration.ToTimeSpan().ToDuration()))
      .ForMember(_ => _.Id, _ => _.Ignore())
      .ForMember(_ => _.Timestamp, _ => _.MapFrom(q => q.Meta.Timestamp.ToDateTimeOffset().ToInstant()))
      .ForMember(_ => _.UserId, _ => _.MapFrom(q => q.Meta.UserId))
      .IgnoreAllPropertiesWithAnInaccessibleSetter();

    CreateMap<ProductCheckedOutEvent, CheckoutInfoData>()
      .ForMember(_ => _.CheckedOutAt, _ => _.MapFrom(q => ProtoTimestamp.FromDateTimeOffset(q.Timestamp.ToDateTimeOffset())))
      .ForMember(_ => _.ProductName, _ => _.MapFrom(q => q.Title))
      .ForMember(_ => _.ProductSku, _ => _.MapFrom(q => q.Sku))
      .ForMember(_ => _.Price, _ => _.MapFrom(q => q.FormattedPrice))
      .ForMember(_ => _.ProductPicture, _ => _.MapFrom(q => q.Picture));

    CreateMap<CheckedOutProductAttr, ProductAttr>().ReverseMap();
    CreateMap<CheckedOutProductLink, ProductLink>().ReverseMap();
    CreateMap<IPagedList<CheckoutInfoData>, CheckoutInfoPagedList>();
  }
}