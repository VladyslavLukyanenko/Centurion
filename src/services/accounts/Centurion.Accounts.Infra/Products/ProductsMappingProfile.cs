using AutoMapper;
using NodaTime;
using Centurion.Accounts.App.Identity.Model;
using Centurion.Accounts.App.Products.Model;
using Centurion.Accounts.App.Products.Services;
using Centurion.Accounts.Core.Products;
using Centurion.Accounts.Infra.Products.Services;

namespace Centurion.Accounts.Infra.Products;

public class ProductsMappingProfile : Profile
{
  public ProductsMappingProfile()
  {
    CreateMap<Dashboard, DashboardData>()
      .ReverseMap()
      .IgnoreAllPropertiesWithAnInaccessibleSetter();

    CreateMap<StripeIntegrationConfig, StripeIntegrationConfig>();
    CreateMap<DiscordConfig, DiscordConfig>();
    CreateMap<DiscordOAuthConfig, DiscordOAuthConfig>();
    CreateMap<HostingConfig, HostingConfig>();

    CreateMap<ProductFeature, ProductFeatureData>()
      .ForMember(_ => _.UploadedIcon, _ => _.Ignore());

    CreateMap<ProductInfo, ProductInfoData>()
      .ForMember(_ => _.UploadedLogo, _ => _.Ignore())
      .ForMember(_ => _.UploadedImage, _ => _.Ignore())
      .ReverseMap()
      .IgnoreAllPropertiesWithAnInaccessibleSetter()
      .ForMember(_ => _.Features, _ => _.Ignore())
      .ForMember(_ => _.LogoSrc, _ => _.Ignore())
      .ForMember(_ => _.ImageSrc, _ => _.Ignore());

    CreateMap<ProductInfo, ProductPublicInfoData>();

    // CreateMap<LicenseKey, LicenseKeyData>();
    CreateMap<DenormalizedLicenseKey, LicenseKeySnapshotData>()
      .ForMember(_ => _.Id, q => q.MapFrom(_ => _.Key.Id))
      .ForMember(_ => _.Value, q => q.MapFrom(_ => _.Key.Value))
      .ForMember(_ => _.Expiry, q => q.MapFrom(_ => _.Key.Expiry))
      // .ForMember(_ => _.ProductId, q => q.MapFrom(_ => _.Key.ProductId))
      // .ForMember(_ => _.LastAuthRequest, q => q.MapFrom(_ => _.Key.LastAuthRequest))
      // .ForMember(_ => _.SessionId, q => q.MapFrom(_ => _.Key.SessionId))
      // .ForMember(_ => _.IsTrial, q => q.MapFrom(_ => _.Key.TrialEndsAt.HasValue))
      // .ForMember(_ => _.IsSuspended, q => q.MapFrom(_ => false))
      //   // _.Key.Suspensions.Count > 0 && _.Key.Suspensions
      //     // .Any(l => !l.End.HasValue || l.End > SystemClock.Instance.GetCurrentInstant())))
      // .ForMember(_ => _.IsUnbindable,
      //   q => q.MapFrom(_ =>
      //     _.Key.UserId.HasValue && _.Key.UnbindableAfter.HasValue
      //                           && _.Key.UnbindableAfter <= SystemClock.Instance.GetCurrentInstant()))
      .ForMember(_ => _.User, q => q.MapFrom(_ => _.User == null
        ? null
        : new UserRef
        {
          Id = _.User.Id,
          Picture = _.User.Avatar,
          FullName = _.User.Name + "#" + _.User.Discriminator
        }))
      .ForMember(_ => _.PlanDesc, q => q.MapFrom(_ => _.Plan.Description))
      .ForMember(_ => _.ReleaseTitle, q => q.MapFrom(_ => _.Release == null ? null : _.Release.Title));

    CreateMap<DenormalizedLicenseKey, PurchasedLicenseKeyData>()
      .IncludeBase<DenormalizedLicenseKey, LicenseKeySnapshotData>()
      .ForMember(_ => _.IsUnbindable,
        q => q.MapFrom(_ => _.Key.UserId.HasValue && _.Key.UnbindableAfter.HasValue
                                                  && _.Key.UnbindableAfter
                                                  <= SystemClock.Instance.GetCurrentInstant()))
      .ForMember(_ => _.HasActiveSession, q => q.MapFrom(_ => _.Key.SessionId != null))
      .ForMember(_ => _.IsExpired,
        q => q.MapFrom(_ => _.Key.Expiry != null && _.Key.Expiry <= SystemClock.Instance.GetCurrentInstant()))
      .ForMember(_ => _.IsSubscriptionCancelled,
        q => q.MapFrom(_ => _.Key.SubscriptionCancelledAt != null
                            && _.Key.SubscriptionCancelledAt <= SystemClock.Instance.GetCurrentInstant()))
      .ForMember(_ => _.LastAuthRequest, q => q.MapFrom(_ => _.Key.LastAuthRequest));


    CreateMap<Release, ReleaseData>()
      .ReverseMap()
      .IgnoreAllPropertiesWithAnInaccessibleSetter();

    CreateMap<Release, SaveReleaseCommand>()
      .ReverseMap()
      .IgnoreAllPropertiesWithAnInaccessibleSetter();

    CreateMap<Plan, PlanData>()
      .ForMember(_ => _.LicenseLifeDays,
        _ => _.MapFrom(q => q.LicenseLife == null ? null : (int?) Math.Floor(q.LicenseLife.Value.TotalDays)))
      .ForMember(_ => _.UnbindableDelayDays,
        _ => _.MapFrom(q => q.UnbindableDelay == null ? null : (int?) Math.Floor(q.UnbindableDelay.Value.TotalDays)))
      .ForMember(_ => _.TrialPeriodDays, _ => _.MapFrom(q => (int) Math.Floor(q.TrialPeriod.TotalDays)))
      .ReverseMap()
      .ForMember(_ => _.LicenseLife,
        _ => _.MapFrom(q =>
          q.LicenseLifeDays == null ? (Duration?) null : Duration.FromDays(q.LicenseLifeDays.Value)))
      .ForMember(_ => _.UnbindableDelay,
        _ => _.MapFrom(q =>
          q.UnbindableDelayDays == null ? (Duration?) null : Duration.FromDays(q.UnbindableDelayDays.Value)))
      .ForMember(_ => _.TrialPeriod, _ => _.MapFrom(q => Duration.FromDays(q.TrialPeriodDays)))
      /*.ForMember(dest => dest.IsUnbindable, opt => opt.MapFrom(src => src.IsUnbindable()))*/;
  }
}