package common

import (
	contract "github.com/CenturionLabs/centurion/checkout-service/contracts"
	"github.com/CenturionLabs/centurion/checkout-service/core"
)

var (
	TaskStatus_PurchaseSucceeded = core.NewTaskStatus(contract.TaskCategory_TASK_CATEGORY_CHECKED_OUT, "Purchased", contract.TaskStage_TASK_STAGE_IDLE)
	TaskStatus_PurchaseDeclined = core.NewTaskStatus(contract.TaskCategory_TASK_CATEGORY_DECLINED, "Declined", contract.TaskStage_TASK_STAGE_ERROR)

  // ProductInStock { get; } = new(TaskStatusCategory.Success, "In Stock", CheckoutStage.Monitoring)
	TaskStatus_ProductInStock = core.NewTaskStatus(contract.TaskCategory_TASK_CATEGORY_UNSPECIFIED, "In Stock", contract.TaskStage_TASK_STAGE_RUNNING)
	TaskStatus_OutOfStock = core.NewTaskStatus(contract.TaskCategory_TASK_CATEGORY_UNSPECIFIED, "Out Of Stock", contract.TaskStage_TASK_STAGE_ERROR)
  TaskStatus_UnknownErrorMonitor   = core.NewTaskStatus(contract.TaskCategory_TASK_CATEGORY_FAILED, "Unknown Error Monitoring", contract.TaskStage_TASK_STAGE_ERROR)
	TaskStatus_UnexpectedFatal   = core.NewTaskStatus(contract.TaskCategory_TASK_CATEGORY_FATAL_ERROR, "Unexpected Fatal Error", contract.TaskStage_TASK_STAGE_IDLE)
)

func TaskStatus_InStock(stage contract.TaskStage) *contract.TaskStatusData {
	return core.NewTaskStatus(contract.TaskCategory_TASK_CATEGORY_UNSPECIFIED, "In Stock", stage)
}

func TaskStatus_PanicFatal(err error) *contract.TaskStatusData {
	return core.NewTaskStatus(contract.TaskCategory_TASK_CATEGORY_FATAL_ERROR, "Fatal: "+err.Error(), contract.TaskStage_TASK_STAGE_IDLE)
}
