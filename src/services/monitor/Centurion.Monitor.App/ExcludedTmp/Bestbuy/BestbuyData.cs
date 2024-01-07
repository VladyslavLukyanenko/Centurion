using System.Text.Json.Serialization;

#nullable disable
namespace Centurion.Monitor.App.Sites.Bestbuy
{
  public partial class BestbuyData
  {
    [JsonPropertyName("sku")] public Sku Sku { get; set; }
  }

  public partial class Sku
  {
    [JsonPropertyName("attributes")] public Attributes Attributes { get; set; }

    [JsonPropertyName("brand")] public Brand Brand { get; set; }

    [JsonPropertyName("buttonState")] public ButtonStateClass ButtonState { get; set; }

    [JsonPropertyName("condition")] public string Condition { get; set; }

    [JsonPropertyName("names")] public Names Names { get; set; }

    [JsonPropertyName("price")] public SkuPrice Price { get; set; }

    [JsonPropertyName("productType")] public string ProductType { get; set; }

    [JsonPropertyName("properties")] public Properties Properties { get; set; }

    [JsonPropertyName("gspAppleCare")] public GspAppleCare[] GspAppleCare { get; set; }

    [JsonPropertyName("skuId")] public string SkuId { get; set; }

    [JsonPropertyName("url")] public string Url { get; set; }

    [JsonPropertyName("class")] public Class Class { get; set; }

    [JsonPropertyName("department")] public Class Department { get; set; }

    [JsonPropertyName("subclass")] public Class Subclass { get; set; }

    [JsonPropertyName("inkSubscriptions")] public object[] InkSubscriptions { get; set; }
  }

  public partial class Attributes
  {
    [JsonPropertyName("lowPriceGuaranteedProduct")]
    public bool LowPriceGuaranteedProduct { get; set; }

    [JsonPropertyName("smartCapable")] public bool? SmartCapable { get; set; }

    [JsonPropertyName("hasEcoRebates")] public bool? HasEcoRebates { get; set; }
  }

  public partial class Brand
  {
    [JsonPropertyName("brand")] public string BrandBrand { get; set; }
  }

  public partial class ButtonStateClass
  {
    [JsonPropertyName("buttonState")] public string ButtonState { get; set; }

    [JsonPropertyName("displayText")] public string DisplayText { get; set; }

    [JsonPropertyName("skuId")] public string SkuId { get; set; }
  }

  public partial class Class
  {
    [JsonPropertyName("displayName")] public string DisplayName { get; set; }

    [JsonPropertyName("id")] public string Id { get; set; }
  }

  public partial class GspAppleCare
  {
    [JsonPropertyName("skuId")] public string SkuId { get; set; }

    [JsonPropertyName("name")] public string Name { get; set; }

    [JsonPropertyName("price")] public GspAppleCarePrice Price { get; set; }

    [JsonPropertyName("type")] public string Type { get; set; }

    [JsonPropertyName("protectionType")] public string ProtectionType { get; set; }

    [JsonPropertyName("term")] public string Term { get; set; }

    [JsonPropertyName("paymentType")] public string PaymentType { get; set; }
  }

  public partial class GspAppleCarePrice
  {
    [JsonPropertyName("currentPrice")] public double CurrentPrice { get; set; }
  }

  public partial class Names
  {
    [JsonPropertyName("short")] public string Short { get; set; }
  }

  public partial class SkuPrice
  {
    [JsonPropertyName("currentPrice")] public decimal CurrentPrice { get; set; }

    [JsonPropertyName("pricingType")] public string PricingType { get; set; }

    [JsonPropertyName("smartPricerEnabled")]
    public bool SmartPricerEnabled { get; set; }

    [JsonPropertyName("priceDomain")] public PriceDomain PriceDomain { get; set; }
  }

  public partial class PriceDomain
  {
    [JsonPropertyName("skuId")] public string SkuId { get; set; }

    [JsonPropertyName("priceEventType")] public string PriceEventType { get; set; }

    [JsonPropertyName("regularPrice")] public double RegularPrice { get; set; }

    [JsonPropertyName("currentPrice")] public double CurrentPrice { get; set; }

    [JsonPropertyName("customerPrice")] public double CustomerPrice { get; set; }

    [JsonPropertyName("totalSavings")] public decimal TotalSavings { get; set; }

    [JsonPropertyName("totalSavingsPercent")]
    public double TotalSavingsPercent { get; set; }

    [JsonPropertyName("isMAP")] public bool IsMap { get; set; }

    [JsonPropertyName("currentAsOfDate")] public string CurrentAsOfDate { get; set; }
  }

  public partial class Properties
  {
    [JsonPropertyName("servicePlansType")] public string ServicePlansType { get; set; }

    [JsonPropertyName("servicePlanDetailsURL")]
    public string ServicePlanDetailsUrl { get; set; }
  }
}