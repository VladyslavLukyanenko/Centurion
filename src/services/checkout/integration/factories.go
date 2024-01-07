package integration

import "github.com/CenturionLabs/centurion/checkout-service/contracts/checkout/integration"

type ProductCheckedOutEventProcessor interface {
	ProcessProductCheckedOutEvent(*integration.ProductCheckedOut)
}
