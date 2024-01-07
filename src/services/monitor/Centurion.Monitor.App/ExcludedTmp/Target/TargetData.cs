using System;
using System.Text.Json.Serialization;

#nullable disable
namespace Centurion.Monitor.App.Sites.Target
{
  namespace QuickType
{
    public partial class TargetData
    {
        [JsonPropertyName("data")]
        public Data Data { get; set; }
    }

    public partial class Data
    {
        [JsonPropertyName("product")]
        public Product Product { get; set; }
    }

    public partial class Product
    {
        [JsonPropertyName("tcin")]
        public string Tcin { get; set; }

        [JsonPropertyName("item")]
        public Item Item { get; set; }

        [JsonPropertyName("price")]
        public Price Price { get; set; }

        [JsonPropertyName("free_shipping")]
        public FreeShipping FreeShipping { get; set; }

        [JsonPropertyName("ratings_and_reviews")]
        public RatingsAndReviews RatingsAndReviews { get; set; }

        [JsonPropertyName("promotions")]
        public Promotion[] Promotions { get; set; }

        [JsonPropertyName("esp")]
        public Esp Esp { get; set; }

        [JsonPropertyName("store_coordinates")]
        public StoreCoordinate[] StoreCoordinates { get; set; }

        [JsonPropertyName("fulfillment")]
        public ProductFulfillment Fulfillment { get; set; }

        [JsonPropertyName("taxonomy")]
        public Taxonomy Taxonomy { get; set; }

        [JsonPropertyName("notify_me_enabled")]
        public bool NotifyMeEnabled { get; set; }

        [JsonPropertyName("ad_placement_url")]
        public Uri AdPlacementUrl { get; set; }
    }

    public partial class Esp
    {
        [JsonPropertyName("esp_group_id")]
        public string EspGroupId { get; set; }

        [JsonPropertyName("tcin")]
        public string Tcin { get; set; }

        [JsonPropertyName("product_description")]
        public EspProductDescription ProductDescription { get; set; }

        [JsonPropertyName("enrichment")]
        public EspEnrichment Enrichment { get; set; }

        [JsonPropertyName("price")]
        public Price Price { get; set; }
    }

    public partial class EspEnrichment
    {
        [JsonPropertyName("images")]
        public PurpleImages Images { get; set; }
    }

    public partial class PurpleImages
    {
        [JsonPropertyName("primary_image_url")]
        public Uri PrimaryImageUrl { get; set; }
    }

    public partial class Price
    {
        [JsonPropertyName("current_retail")]
        public double CurrentRetail { get; set; }

        [JsonPropertyName("default_price")]
        public bool DefaultPrice { get; set; }

        [JsonPropertyName("formatted_current_price")]
        public string FormattedCurrentPrice { get; set; }

        [JsonPropertyName("formatted_current_price_type")]
        public string FormattedCurrentPriceType { get; set; }

        [JsonPropertyName("is_current_price_range")]
        public bool IsCurrentPriceRange { get; set; }

        [JsonPropertyName("location_id")]
        public object LocationId { get; set; }

        [JsonPropertyName("msrp")]
        public double Msrp { get; set; }

        [JsonPropertyName("reg_retail")]
        public double RegRetail { get; set; }
    }

    public partial class EspProductDescription
    {
        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("bullet_descriptions")]
        public string[] BulletDescriptions { get; set; }
    }

    public partial class FreeShipping
    {
        [JsonPropertyName("enabled")]
        public bool Enabled { get; set; }
    }

    public partial class ProductFulfillment
    {
        [JsonPropertyName("shipping_options")]
        public ShippingOptions ShippingOptions { get; set; }

        [JsonPropertyName("store_options")]
        public StoreOption[] StoreOptions { get; set; }

        [JsonPropertyName("scheduled_delivery")]
        public ScheduledDelivery ScheduledDelivery { get; set; }
    }

    public partial class ScheduledDelivery
    {
        [JsonPropertyName("availability_status")]
        public string AvailabilityStatus { get; set; }

        [JsonPropertyName("location_available_to_promise_quantity")]
        public object LocationAvailableToPromiseQuantity { get; set; }
    }

    public partial class ShippingOptions
    {
        [JsonPropertyName("availability_status")]
        public string AvailabilityStatus { get; set; }

        [JsonPropertyName("loyalty_availability_status")]
        public string LoyaltyAvailabilityStatus { get; set; }

        [JsonPropertyName("available_to_promise_quantity")]
        public object AvailableToPromiseQuantity { get; set; }

        [JsonPropertyName("minimum_order_quantity")]
        public object MinimumOrderQuantity { get; set; }

        [JsonPropertyName("reason_code")]
        public string ReasonCode { get; set; }

        [JsonPropertyName("services")]
        public object[] Services { get; set; }
    }

    public partial class StoreOption
    {
        [JsonPropertyName("location_name")]
        public string LocationName { get; set; }

        [JsonPropertyName("location_id")]
        public object LocationId { get; set; }

        [JsonPropertyName("search_response_store_type")]
        public string SearchResponseStoreType { get; set; }

        [JsonPropertyName("location_available_to_promise_quantity")]
        public object LocationAvailableToPromiseQuantity { get; set; }

        [JsonPropertyName("order_pickup")]
        public Curbside OrderPickup { get; set; }

        [JsonPropertyName("curbside")]
        public Curbside Curbside { get; set; }

        [JsonPropertyName("in_store_only")]
        public Curbside InStoreOnly { get; set; }

        [JsonPropertyName("ship_to_store")]
        public Curbside ShipToStore { get; set; }
    }

    public partial class Curbside
    {
        [JsonPropertyName("availability_status")]
        public string AvailabilityStatus { get; set; }
    }

    public partial class Item
    {
        [JsonPropertyName("dpci")]
        public string Dpci { get; set; }

        [JsonPropertyName("assigned_selling_channels_code")]
        public string AssignedSellingChannelsCode { get; set; }

        [JsonPropertyName("primary_barcode")]
        public string PrimaryBarcode { get; set; }

        [JsonPropertyName("product_classification")]
        public ProductClassification ProductClassification { get; set; }

        [JsonPropertyName("product_description")]
        public ItemProductDescription ProductDescription { get; set; }

        [JsonPropertyName("compliance")]
        public Compliance Compliance { get; set; }

        [JsonPropertyName("enrichment")]
        public ItemEnrichment Enrichment { get; set; }

        [JsonPropertyName("relationship_type_code")]
        public string RelationshipTypeCode { get; set; }

        [JsonPropertyName("fulfillment")]
        public ItemFulfillment Fulfillment { get; set; }

        [JsonPropertyName("product_vendors")]
        public ProductVendor[] ProductVendors { get; set; }

        [JsonPropertyName("merchandise_type_attributes")]
        public MerchandiseTypeAttribute[] MerchandiseTypeAttributes { get; set; }

        [JsonPropertyName("wellness_merchandise_attributes")]
        public WellnessMerchandiseAttribute[] WellnessMerchandiseAttributes { get; set; }

        [JsonPropertyName("mmbv_content")]
        public MmbvContent MmbvContent { get; set; }

        [JsonPropertyName("eligibility_rules")]
        public EligibilityRules EligibilityRules { get; set; }

        [JsonPropertyName("handling")]
        public Handling Handling { get; set; }

        [JsonPropertyName("package_dimensions")]
        public PackageDimensions PackageDimensions { get; set; }

        [JsonPropertyName("environmental_segmentation")]
        public EnvironmentalSegmentation EnvironmentalSegmentation { get; set; }

        [JsonPropertyName("formatted_return_method")]
        public string FormattedReturnMethod { get; set; }

        [JsonPropertyName("return_policies_guest_message")]
        public string ReturnPoliciesGuestMessage { get; set; }

        [JsonPropertyName("return_policy_url")]
        public Uri ReturnPolicyUrl { get; set; }

        [JsonPropertyName("cart_add_on_threshold")]
        public object CartAddOnThreshold { get; set; }
    }

    public partial class Compliance
    {
        [JsonPropertyName("is_proposition_65")]
        public bool IsProposition65 { get; set; }
    }

    public partial class EligibilityRules
    {
    }

    public partial class ItemEnrichment
    {
        [JsonPropertyName("buy_url")]
        public Uri BuyUrl { get; set; }

        [JsonPropertyName("images")]
        public FluffyImages Images { get; set; }

        [JsonPropertyName("videos")]
        public Video[] Videos { get; set; }
    }

    public partial class FluffyImages
    {
        [JsonPropertyName("primary_image_url")]
        public string PrimaryImageUrl { get; set; }

        [JsonPropertyName("alternate_image_urls")]
        public Uri[] AlternateImageUrls { get; set; }

        [JsonPropertyName("content_labels")]
        public ContentLabel[] ContentLabels { get; set; }
    }

    public partial class ContentLabel
    {
        [JsonPropertyName("image_url")]
        public Uri ImageUrl { get; set; }
    }

    public partial class Video
    {
        [JsonPropertyName("video_captions")]
        public VideoCaption[] VideoCaptions { get; set; }

        [JsonPropertyName("video_title")]
        public string VideoTitle { get; set; }

        public VideoFile[] VideoFiles { get; set; }

        [JsonPropertyName("video_poster_image")]
        public Uri VideoPosterImage { get; set; }

        [JsonPropertyName("video_length_seconds")]
        public string VideoLengthSeconds { get; set; }
    }

    public partial class VideoCaption
    {
        [JsonPropertyName("caption_url")]
        public Uri CaptionUrl { get; set; }
    }

    public partial class VideoFile
    {
        [JsonPropertyName("video_url")]
        public Uri VideoUrl { get; set; }
    }

    public partial class EnvironmentalSegmentation
    {
        [JsonPropertyName("is_hazardous_material")]
        public bool IsHazardousMaterial { get; set; }
    }

    public partial class ItemFulfillment
    {
        [JsonPropertyName("purchase_limit")]
        public long PurchaseLimit { get; set; }

        [JsonPropertyName("is_ship_in_original_container")]
        public bool IsShipInOriginalContainer { get; set; }

        [JsonPropertyName("po_box_prohibited_message")]
        public string PoBoxProhibitedMessage { get; set; }

        [JsonPropertyName("shipping_exclusion_codes")]
        public string[] ShippingExclusionCodes { get; set; }
    }

    public partial class Handling
    {
        [JsonPropertyName("buy_unit_of_measure")]
        public string BuyUnitOfMeasure { get; set; }

        [JsonPropertyName("import_designation_description")]
        public string ImportDesignationDescription { get; set; }
    }

    public partial class MerchandiseTypeAttribute
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("values")]
        public Value[] Values { get; set; }
    }

    public partial class Value
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }
    }

    public partial class MmbvContent
    {
        [JsonPropertyName("street_date")]
        public DateTimeOffset StreetDate { get; set; }
    }

    public partial class PackageDimensions
    {
        [JsonPropertyName("weight")]
        public double Weight { get; set; }

        [JsonPropertyName("weight_unit_of_measure")]
        public string WeightUnitOfMeasure { get; set; }

        [JsonPropertyName("width")]
        public double Width { get; set; }

        [JsonPropertyName("depth")]
        public double Depth { get; set; }

        [JsonPropertyName("height")]
        public double Height { get; set; }

        [JsonPropertyName("dimension_unit_of_measure")]
        public string DimensionUnitOfMeasure { get; set; }
    }

    public partial class ProductClassification
    {
        [JsonPropertyName("product_type_name")]
        public string ProductTypeName { get; set; }
    }

    public partial class ItemProductDescription
    {
        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("downstream_description")]
        public string DownstreamDescription { get; set; }

        [JsonPropertyName("bullet_descriptions")]
        public string[] BulletDescriptions { get; set; }

        [JsonPropertyName("soft_bullets")]
        public SoftBullets SoftBullets { get; set; }
    }

    public partial class SoftBullets
    {
        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("bullets")]
        public string[] Bullets { get; set; }
    }

    public partial class ProductVendor
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("vendor_name")]
        public string VendorName { get; set; }
    }

    public partial class WellnessMerchandiseAttribute
    {
        [JsonPropertyName("badge_url")]
        public Uri BadgeUrl { get; set; }

        [JsonPropertyName("parent_id")]
        public string ParentId { get; set; }

        [JsonPropertyName("parent_name")]
        public string ParentName { get; set; }

        [JsonPropertyName("value_id")]
        public string ValueId { get; set; }

        [JsonPropertyName("value_name")]
        public string ValueName { get; set; }

        [JsonPropertyName("wellness_description")]
        public string WellnessDescription { get; set; }
    }

    public partial class Promotion
    {
        [JsonPropertyName("pdp_message")]
        public string PdpMessage { get; set; }

        [JsonPropertyName("plp_message")]
        public string PlpMessage { get; set; }

        [JsonPropertyName("subscription_type")]
        public string SubscriptionType { get; set; }

        [JsonPropertyName("threshold_type")]
        public string ThresholdType { get; set; }

        [JsonPropertyName("threshold_value")]
        public object ThresholdValue { get; set; }

        [JsonPropertyName("applied_location_id")]
        public object AppliedLocationId { get; set; }

        [JsonPropertyName("channel")]
        public string Channel { get; set; }

        [JsonPropertyName("legal_disclaimer_text")]
        public string LegalDisclaimerText { get; set; }

        [JsonPropertyName("promotion_id")]
        public string PromotionId { get; set; }

        [JsonPropertyName("promotion_class")]
        public string PromotionClass { get; set; }

        [JsonPropertyName("global_subscription_flag")]
        public bool GlobalSubscriptionFlag { get; set; }

        [JsonPropertyName("circle_offer")]
        public bool CircleOffer { get; set; }
    }

    public partial class RatingsAndReviews
    {
        [JsonPropertyName("statistics")]
        public Statistics Statistics { get; set; }
    }

    public partial class Statistics
    {
        [JsonPropertyName("question_count")]
        public long QuestionCount { get; set; }

        [JsonPropertyName("rating")]
        public Rating Rating { get; set; }

        [JsonPropertyName("review_count")]
        public long ReviewCount { get; set; }
    }

    public partial class Rating
    {
        [JsonPropertyName("average")]
        public double Average { get; set; }

        [JsonPropertyName("count")]
        public long Count { get; set; }
    }

    public partial class StoreCoordinate
    {
        [JsonPropertyName("aisle")]
        public long Aisle { get; set; }

        [JsonPropertyName("block")]
        public string Block { get; set; }

        [JsonPropertyName("floor")]
        public object Floor { get; set; }

        [JsonPropertyName("x")]
        public double X { get; set; }

        [JsonPropertyName("y")]
        public double Y { get; set; }
    }

    public partial class Taxonomy
    {
        [JsonPropertyName("category")]
        public Category Category { get; set; }

        [JsonPropertyName("breadcrumbs")]
        public Category[] Breadcrumbs { get; set; }
    }

    public partial class Category
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("node_id")]
        public string NodeId { get; set; }
    }
}

}