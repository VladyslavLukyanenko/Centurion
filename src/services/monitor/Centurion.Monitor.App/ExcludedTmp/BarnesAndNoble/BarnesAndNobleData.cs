using System.Text.Json.Serialization;

#nullable disable
namespace Centurion.Monitor.App.Sites.BarnesAndNoble
{
  public class BarnesAndNobleData
  {
    [JsonPropertyName("product")] public BarnesAndNobleProductData Product { get; set; }
  }

  public class BarnesAndNobleProductData
  {
    [JsonPropertyName("attributes")] public BarnesAndNobleProductAttrsData Attributes { get; set; }
  }

  public class BarnesAndNobleProductAttrsData
  {
    [JsonPropertyName("outOfStock")] public bool OutOfStock { get; set; }
  }


  /*
type zresponse struct {
	Product struct {
		Category struct {
			ProductType string `json:"productType"`
		} `json:"category"`
		Price struct {
			BasePrice int `json:"basePrice"`
		} `json:"price"`
		ProductInfo struct {
			ProductImage     string `json:"productImage"`
			DepartmentCode   string `json:"departmentCode"`
			Sku              string `json:"sku"`
			ProductName      string `json:"productName"`
			ProductID        string `json:"productID"`
			ProductURL       string `json:"productURL"`
			ProductThumbnail string `json:"productThumbnail"`
		} `json:"productInfo"`
		Attributes struct {
			OutOfStock        bool    `json:"outOfStock"`
			Reviews           int     `json:"reviews"`
			IsDeviceProduct   bool    `json:"isDeviceProduct"`
			IsDigitalProduct  bool    `json:"isDigitalProduct"`
			IsEbookProduct    bool    `json:"isEbookProduct"`
			IsPhysicalProduct bool    `json:"isPhysicalProduct"`
			Rating            float64 `json:"rating"`
			IsIosAvailable    bool    `json:"isIosAvailable"`
			IsNOOKProduct     bool    `json:"isNOOKProduct"`
			NotifyWhenStocked bool    `json:"notifyWhenStocked"`
		} `json:"attributes"`
	} `json:"product"`
}*/
}