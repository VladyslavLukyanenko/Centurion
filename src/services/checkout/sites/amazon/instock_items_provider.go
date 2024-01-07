package amazon

type InStockItemsProvider interface {
	GetRandomSKU(region amazonRegion) string
}