package core

import (
	integContracts "github.com/CenturionLabs/centurion/checkout-service/contracts/checkout/integration"
	"github.com/CenturionLabs/centurion/checkout-service/contracts/common"
	"google.golang.org/protobuf/types/known/durationpb"
	"google.golang.org/protobuf/types/known/timestamppb"
	"strconv"
	"time"
)

func CreateProductCheckedOutEvent(spawnedTask *SpawnedTask) *integContracts.ProductCheckedOut {
	t := spawnedTask.Payload

	profileName := "<Unknown!!>"
  if p := spawnedTask.CheckoutTask.GetUsedProfile(); p != nil {
    profileName = p.Name
  } else if len(t.ProfileList) == 1 {
		profileName = t.ProfileList[0].Name
	}

	event := &integContracts.ProductCheckedOut{
		Meta: &common.EventMetadata{
			Timestamp: timestamppb.New(time.Now().UTC()),
			TaskId:    spawnedTask.Payload.Id,
			UserId:    &spawnedTask.UserId,
		},
		Title:     t.Product.Name,
		Picture:   t.Product.Image,
		Thumbnail: t.Product.Image,
		Url:       t.Product.Link,
		//Mode:          t.Mode,
		//Qty:           t.Quantity,
		//Delay:         t.Delay,
		Profile:       profileName,
		Store:         t.Module.String(),
		ProcessingLog: spawnedTask.Logger.ToMessagesSlice(),
		TaskId:        t.Id,
		UserId:        spawnedTask.UserId,
		Sku:           t.Product.Sku,
		Duration:      durationpb.New(spawnedTask.EndTime.Sub(spawnedTask.StartTime)),
	}

	if t.Product.Price != nil {
		event.Price = *t.Product.Price
		event.FormattedPrice = "$" + strconv.FormatFloat(*t.Product.Price, 'f', 2, 64)
	}

	return event
}
