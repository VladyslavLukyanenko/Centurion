using System.Reflection;

#nullable disable

namespace Centurion.Cli.Core.Services.Profiles;

[Obfuscation(Feature = "renaming", ApplyToMembers = true)]
public class CsvProfileData
{
  public string GroupName { get; set; }

  public string Name { get; set; }
  public string FirstName { get; set; }
  public string LastName { get; set; }
  public string PhoneNumber { get; set; }
  public bool BillingAsShipping { get; set; }

  public string BillingAddressLine1 { get; set; }
  public string BillingAddressLine2 { get; set; }
  public string BillingAddressCity { get; set; }
  public string BillingAddressZipCode { get; set; }
  public string BillingAddressCountryId { get; set; }
  public string BillingAddressProvinceCode { get; set; }

  public string ShippingAddressLine1 { get; set; }
  public string ShippingAddressLine2 { get; set; }
  public string ShippingAddressCity { get; set; }
  public string ShippingAddressZipCode { get; set; }
  public string ShippingAddressCountryId { get; set; }
  public string ShippingAddressProvinceCode { get; set; }

  public string BillingCvv { get; set; }
  public int BillingExpirationMonth { get; set; }
  public int BillingExpirationYear { get; set; }
  public string BillingCardNumber { get; set; }
  public string BillingHolderName { get; set; }
}