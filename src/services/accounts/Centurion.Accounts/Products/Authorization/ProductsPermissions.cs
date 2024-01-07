using Centurion.Accounts.Foundation.Authorization;

// ReSharper disable once CheckNamespace
namespace Centurion.Accounts.Authorization;

public partial class Permissions
{
  [PermissionDescription("Analytics read general")]
  public const string AnalyticsGeneralRead = nameof(AnalyticsGeneralRead);

  [PermissionDescription("Analytics read discord")]
  public const string AnalyticsDiscordRead = nameof(AnalyticsDiscordRead);


  [PermissionDescription("Licenses suspend")]
  public const string LicenseKeysToggleSuspend = nameof(LicenseKeysToggleSuspend);

  [PermissionDescription("Licenses stats")]
  public const string LicenseKeysStatsUsedCount = nameof(LicenseKeysStatsUsedCount);

  [PermissionDescription("Licenses management")]
  public const string LicenseKeysManage = nameof(LicenseKeysManage);

  [PermissionDescription("Licenses delete")]
  public const string LicenseKeysDelete = nameof(LicenseKeysDelete);


  [PermissionDescription("Plans management")]
  public const string PlansManage = nameof(PlansManage);

  [PermissionDescription("Plans delete")]
  public const string PlansDelete = nameof(PlansDelete);


  [PermissionDescription("Release management")]
  public const string ReleaseManage = nameof(ReleaseManage);

  [PermissionDescription("Release delete")]
  public const string ReleaseDelete = nameof(ReleaseDelete);


  [PermissionDescription("Roles management")]
  public const string RolesManage = nameof(RolesManage);

  [PermissionDescription("Roles delete")]
  public const string RolesDelete = nameof(RolesDelete);


  [PermissionDescription("Staff management")]
  public const string StaffManage = nameof(StaffManage);

  [PermissionDescription("Staff delete")]
  public const string StaffDelete = nameof(StaffDelete);


  [PermissionDescription("Broadcast announces")]
  public const string AnnouncesBroadcast = nameof(AnnouncesBroadcast);
}