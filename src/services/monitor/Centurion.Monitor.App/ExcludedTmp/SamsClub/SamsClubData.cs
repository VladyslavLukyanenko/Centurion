using System;
using System.Text.Json.Serialization;

#nullable disable
namespace Centurion.Monitor.App.Sites.SamsClub
{
  public partial class SamsClubData
  {
    [JsonPropertyName("status")] public string Status { get; set; }

    [JsonPropertyName("payload")] public Payload Payload { get; set; }
  }

  public partial class Payload
  {
    [JsonPropertyName("products")] public Product[] Products { get; set; }
  }

  public partial class Product
  {
    [JsonPropertyName("productId")] public string ProductId { get; set; }

    [JsonPropertyName("productType")] public string ProductType { get; set; }

    [JsonPropertyName("category")] public Category Category { get; set; }

    [JsonPropertyName("searchAndSeo")] public SearchAndSeo SearchAndSeo { get; set; }

    [JsonPropertyName("shippingOption")] public ProductShippingOption ShippingOption { get; set; }

    [JsonPropertyName("manufacturingInfo")]
    public ProductManufacturingInfo ManufacturingInfo { get; set; }

    [JsonPropertyName("descriptors")] public ProductDescriptors Descriptors { get; set; }

    [JsonPropertyName("reviewsAndRatings")]
    public ReviewsAndRatings ReviewsAndRatings { get; set; }

    [JsonPropertyName("status")] public string Status { get; set; }

    [JsonPropertyName("skus")] public Skus[] Skus { get; set; }

    [JsonPropertyName("expiresAt")] public DateTimeOffset ExpiresAt { get; set; }

    [JsonPropertyName("cached")] public bool Cached { get; set; }

    [JsonPropertyName("id")] public string Id { get; set; }

    [JsonPropertyName("prodProperties")] public string[] ProdProperties { get; set; }

    [JsonPropertyName("legacyProductType")]
    public string LegacyProductType { get; set; }

    [JsonPropertyName("parentCategories")]
    public string[] ParentCategories { get; set; }

    [JsonPropertyName("lastModifiedAt")] public DateTimeOffset LastModifiedAt { get; set; }
  }

  public partial class Category
  {
    [JsonPropertyName("name")] public string Name { get; set; }

    [JsonPropertyName("categoryId")]
    // [JsonConverter(typeof(PurpleParseStringConverter))]
    public string CategoryId { get; set; }

    [JsonPropertyName("breadcrumbs")] public Breadcrumb[] Breadcrumbs { get; set; }

    [JsonPropertyName("seoUrl")] public string SeoUrl { get; set; }
  }

  public partial class Breadcrumb
  {
    [JsonPropertyName("displayName")] public string DisplayName { get; set; }

    [JsonPropertyName("navId")] public long NavId { get; set; }

    [JsonPropertyName("seoUrl")] public string SeoUrl { get; set; }
  }

  public partial class ProductDescriptors
  {
    [JsonPropertyName("name")] public string Name { get; set; }

    [JsonPropertyName("shortDescription")] public string ShortDescription { get; set; }

    [JsonPropertyName("longDescription")] public string LongDescription { get; set; }

    [JsonPropertyName("whyWeLoveIt")] public string WhyWeLoveIt { get; set; }
  }

  public partial class ProductManufacturingInfo
  {
    [JsonPropertyName("model")]
    // [JsonConverter(typeof(PurpleParseStringConverter))]
    public string Model { get; set; }

    [JsonPropertyName("brand")] public string Brand { get; set; }

    [JsonPropertyName("assembledCountry")] public string AssembledCountry { get; set; }

    [JsonPropertyName("assembledSize")] public string AssembledSize { get; set; }

    [JsonPropertyName("componentCountry")] public string ComponentCountry { get; set; }

    [JsonPropertyName("specification")] public string Specification { get; set; }

    [JsonPropertyName("warranty")] public string Warranty { get; set; }

    [JsonPropertyName("salesTaxCode")] public long SalesTaxCode { get; set; }

    [JsonPropertyName("additionalInfo")] public string AdditionalInfo { get; set; }
  }

  public partial class ReviewsAndRatings
  {
    [JsonPropertyName("numReviews")] public long NumReviews { get; set; }

    [JsonPropertyName("avgRating")] public double AvgRating { get; set; }

    [JsonPropertyName("isEligibleForAskAndAnswer")]
    public bool IsEligibleForAskAndAnswer { get; set; }
  }

  public partial class SearchAndSeo
  {
    [JsonPropertyName("keywords")] public string Keywords { get; set; }

    [JsonPropertyName("name")] public string Name { get; set; }

    [JsonPropertyName("url")] public string Url { get; set; }
  }

  public partial class ProductShippingOption
  {
    [JsonPropertyName("info")] public string Info { get; set; }
  }

  public partial class Skus
  {
    [JsonPropertyName("skuId")] public string SkuId { get; set; }

    [JsonPropertyName("productId")] public string ProductId { get; set; }

    [JsonPropertyName("merchandiseFineLine")]
    // [JsonConverter(typeof(PurpleParseStringConverter))]
    public string MerchandiseFineLine { get; set; }

    [JsonPropertyName("descriptors")] public SkusDescriptors Descriptors { get; set; }

    [JsonPropertyName("assets")] public Assets Assets { get; set; }

    [JsonPropertyName("restriction")] public Restriction[] Restriction { get; set; }

    [JsonPropertyName("returnInfo")] public ReturnInfo ReturnInfo { get; set; }

    [JsonPropertyName("shippingOption")] public SkusShippingOption ShippingOption { get; set; }

    [JsonPropertyName("skuLogistics")] public SkuLogistics SkuLogistics { get; set; }

    [JsonPropertyName("onlineOffer")] public OnlineOffer OnlineOffer { get; set; }

    [JsonPropertyName("status")] public string Status { get; set; }

    [JsonPropertyName("fulfillmentChannels")]
    public string[] FulfillmentChannels { get; set; }

    [JsonPropertyName("merchandiseCategory")]
    public long MerchandiseCategory { get; set; }

    [JsonPropertyName("merchandiseSubCategory")]
    public long MerchandiseSubCategory { get; set; }

    [JsonPropertyName("manufacturingInfo")]
    public SkusManufacturingInfo ManufacturingInfo { get; set; }

    [JsonPropertyName("shippingMethods")] public string[] ShippingMethods { get; set; }

    [JsonPropertyName("skuProperties")] public string[] SkuProperties { get; set; }
  }

  public partial class Assets
  {
    [JsonPropertyName("image")] public string Image { get; set; }
  }

  public partial class SkusDescriptors
  {
    [JsonPropertyName("name")] public string Name { get; set; }
  }

  public partial class SkusManufacturingInfo
  {
    [JsonPropertyName("model")]
    // [JsonConverter(typeof(PurpleParseStringConverter))]
    public string Model { get; set; }
  }

  public partial class OnlineOffer
  {
    [JsonPropertyName("clubId")] public long ClubId { get; set; }

    [JsonPropertyName("itemNumber")]
    // [JsonConverter(typeof(PurpleParseStringConverter))]
    public string ItemNumber { get; set; }

    [JsonPropertyName("gtin")] public string Gtin { get; set; }

    [JsonPropertyName("mdsFamId")]
    // [JsonConverter(typeof(PurpleParseStringConverter))]
    public string MdsFamId { get; set; }

    [JsonPropertyName("offerId")] public string OfferId { get; set; }

    [JsonPropertyName("skuId")] public string SkuId { get; set; }

    [JsonPropertyName("offerStatus")] public string OfferStatus { get; set; }

    [JsonPropertyName("generatedUPC")] public string GeneratedUpc { get; set; }

    [JsonPropertyName("variableWeightIndicator")]
    // [JsonConverter(typeof(FluffyParseStringConverter))]
    public string VariableWeightIndicator { get; set; }

    [JsonPropertyName("sellQtyUnitofMeasure")]
    public string SellQtyUnitofMeasure { get; set; }

    [JsonPropertyName("vendor")] public Vendor Vendor { get; set; }

    [JsonPropertyName("inventory")] public Inventory Inventory { get; set; }

    [JsonPropertyName("id")] public string Id { get; set; }

    [JsonPropertyName("price")] public Price Price { get; set; }

    [JsonPropertyName("productId")] public string ProductId { get; set; }

    [JsonPropertyName("onlineOfferProperties")]
    public string[] OnlineOfferProperties { get; set; }

    [JsonPropertyName("inventorySourceNetwork")]
    public string InventorySourceNetwork { get; set; }
  }

  public partial class Inventory
  {
    [JsonPropertyName("status")] public string Status { get; set; }

    [JsonPropertyName("availableToSellQuantity")]
    public long AvailableToSellQuantity { get; set; }

    [JsonPropertyName("qtyLeft")] public long QtyLeft { get; set; }

    [JsonPropertyName("qtySold")] public long QtySold { get; set; }
  }

  public partial class Price
  {
    [JsonPropertyName("type")] public string Type { get; set; }

    [JsonPropertyName("startPrice")] public FinalPriceClass StartPrice { get; set; }

    [JsonPropertyName("finalPrice")] public FinalPriceClass FinalPrice { get; set; }

    [JsonPropertyName("priceDisplayOption")]
    public string PriceDisplayOption { get; set; }

    [JsonPropertyName("priceStatus")] public string PriceStatus { get; set; }
  }

  public partial class FinalPriceClass
  {
    [JsonPropertyName("amount")] public double Amount { get; set; }

    [JsonPropertyName("currency")] public string Currency { get; set; }
  }

  public partial class Vendor
  {
    [JsonPropertyName("stockId")]
    // [JsonConverter(typeof(PurpleParseStringConverter))]
    public string StockId { get; set; }

    [JsonPropertyName("number")]
    // [JsonConverter(typeof(PurpleParseStringConverter))]
    public string Number { get; set; }

    [JsonPropertyName("departmentNumber")]
    // [JsonConverter(typeof(PurpleParseStringConverter))]
    public string DepartmentNumber { get; set; }

    [JsonPropertyName("sequenceNumber")]
    // [JsonConverter(typeof(PurpleParseStringConverter))]
    public string SequenceNumber { get; set; }

    [JsonPropertyName("address")] public Address[] Address { get; set; }

    [JsonPropertyName("name")] public string Name { get; set; }
  }

  public partial class Address
  {
    [JsonPropertyName("locationId")]
    // [JsonConverter(typeof(PurpleParseStringConverter))]
    public string LocationId { get; set; }

    [JsonPropertyName("type")] public string Type { get; set; }

    [JsonPropertyName("addressLine1")] public string AddressLine1 { get; set; }

    [JsonPropertyName("city")] public string City { get; set; }

    [JsonPropertyName("state")] public string State { get; set; }

    [JsonPropertyName("postalCode")]
    // [JsonConverter(typeof(PurpleParseStringConverter))]
    public string PostalCode { get; set; }

    [JsonPropertyName("county")] public string County { get; set; }

    [JsonPropertyName("country")] public string Country { get; set; }

    [JsonPropertyName("shipNode")] public string ShipNode { get; set; }

    [JsonPropertyName("shipNodeType")] public string ShipNodeType { get; set; }
  }

  public partial class Restriction
  {
    [JsonPropertyName("type")] public string Type { get; set; }

    [JsonPropertyName("shipCriterionName")]
    public string ShipCriterionName { get; set; }

    [JsonPropertyName("states")] public string States { get; set; }
  }

  public partial class ReturnInfo
  {
    [JsonPropertyName("returnLocation")] public string ReturnLocation { get; set; }

    [JsonPropertyName("returnDays")]
    // [JsonConverter(typeof(PurpleParseStringConverter))]
    public string ReturnDays { get; set; }
  }

  public partial class SkusShippingOption
  {
    [JsonPropertyName("info")]
    // [JsonConverter(typeof(PurpleParseStringConverter))]
    public string Info { get; set; }

    [JsonPropertyName("costType")] public string CostType { get; set; }

    [JsonPropertyName("flag")] public string Flag { get; set; }

    [JsonPropertyName("isAirShippable")] public bool IsAirShippable { get; set; }
  }

  public partial class SkuLogistics
  {
    [JsonPropertyName("length")] public Height Length { get; set; }

    [JsonPropertyName("width")] public Height Width { get; set; }

    [JsonPropertyName("height")] public Height Height { get; set; }

    [JsonPropertyName("weight")] public Height Weight { get; set; }

    [JsonPropertyName("shipAsIsFlag")] public bool ShipAsIsFlag { get; set; }

    [JsonPropertyName("numberOfBoxes")] public long NumberOfBoxes { get; set; }
  }

  public partial class Height
  {
    [JsonPropertyName("value")] public double Value { get; set; }

    [JsonPropertyName("unitOfMeasure")] public string UnitOfMeasure { get; set; }
  }
}