package yeezysupply

type ProductSize struct {
  VariationList []*ProductVariation `json:"variation_list"`
}

type ProductVariation struct {
  Availability int32 `json:"availability"`
  Sku string `json:"sku"`
  Size string `json:"size"`
}
