package yeezysupply

import (
	contract "github.com/CenturionLabs/centurion/checkout-service/contracts"
	"github.com/CenturionLabs/centurion/checkout-service/core"
  "strconv"
)

var (
	taskStatus_CheckedOut                         = core.NewTaskStatus(contract.TaskCategory_TASK_CATEGORY_CHECKED_OUT, "Checked Out", contract.TaskStage_TASK_STAGE_IDLE)
	taskStatus_NoScripts                          = core.NewTaskStatus(contract.TaskCategory_TASK_CATEGORY_FAILED, "No Scripts", contract.TaskStage_TASK_STAGE_ERROR)
	taskStatus_HomePageLoaded                     = core.NewTaskStatus(contract.TaskCategory_TASK_CATEGORY_UNSPECIFIED, "Home page loaded", contract.TaskStage_TASK_STAGE_RUNNING)
	taskStatus_FoundSensor                        = core.NewTaskStatus(contract.TaskCategory_TASK_CATEGORY_UNSPECIFIED, "Found sensor", contract.TaskStage_TASK_STAGE_RUNNING)
	taskStatus_CantVisitHomePage                  = core.NewTaskStatus(contract.TaskCategory_TASK_CATEGORY_FAILED, "Can't visit home page", contract.TaskStage_TASK_STAGE_ERROR)
	taskStatus_ProxyBanCantFetchBloom             = core.NewTaskStatus(contract.TaskCategory_TASK_CATEGORY_FAILED, "Proxy ban. Can't get bloom", contract.TaskStage_TASK_STAGE_ERROR)
	taskStatus_ProxyBanCantVisitProdPage          = core.NewTaskStatus(contract.TaskCategory_TASK_CATEGORY_FAILED, "Proxy ban. Can't visit prod page", contract.TaskStage_TASK_STAGE_ERROR)
	taskStatus_CantFetchBloomDueToUnexpectedError = core.NewTaskStatus(contract.TaskCategory_TASK_CATEGORY_FAILED, "Unexpected error. Can't get bloom", contract.TaskStage_TASK_STAGE_ERROR)
	taskStatus_ProxyBanRestart                    = core.NewTaskStatus(contract.TaskCategory_TASK_CATEGORY_FAILED, "Proxy ban. Restarting...", contract.TaskStage_TASK_STAGE_ERROR)
	taskStatus_NoReply                            = core.NewTaskStatus(contract.TaskCategory_TASK_CATEGORY_FAILED, "No reply", contract.TaskStage_TASK_STAGE_ERROR)
	taskStatus_BanUserInfoNo1                     = core.NewTaskStatus(contract.TaskCategory_TASK_CATEGORY_FAILED, "Ban at get user info No1", contract.TaskStage_TASK_STAGE_ERROR)
	taskStatus_FailedUserInfoNo1                  = core.NewTaskStatus(contract.TaskCategory_TASK_CATEGORY_FAILED, "Failed to secondary prod page", contract.TaskStage_TASK_STAGE_ERROR)
	taskStatus_FailedFetchSizes                   = core.NewTaskStatus(contract.TaskCategory_TASK_CATEGORY_FAILED, "Failed to fetch sizes", contract.TaskStage_TASK_STAGE_ERROR)
	taskStatus_FailedAddToCart                    = core.NewTaskStatus(contract.TaskCategory_TASK_CATEGORY_FAILED, "Failed add to cart", contract.TaskStage_TASK_STAGE_ERROR)
	taskStatus_PageNotFound                       = core.NewTaskStatus(contract.TaskCategory_TASK_CATEGORY_FAILED, "Page not found", contract.TaskStage_TASK_STAGE_ERROR)
	taskStatus_CantVisitFW                        = core.NewTaskStatus(contract.TaskCategory_TASK_CATEGORY_FAILED, "Can't visit FW", contract.TaskStage_TASK_STAGE_ERROR)
	taskStatus_FailedSendSensor                   = core.NewTaskStatus(contract.TaskCategory_TASK_CATEGORY_FAILED, "Failed to send sensor", contract.TaskStage_TASK_STAGE_ERROR)
	taskStatus_Forbid                             = core.NewTaskStatus(contract.TaskCategory_TASK_CATEGORY_FAILED, "Failed. Access forbidden", contract.TaskStage_TASK_STAGE_ERROR)
	taskStatus_CantInitializePx                   = core.NewTaskStatus(contract.TaskCategory_TASK_CATEGORY_FAILED, "Can't initialize PX", contract.TaskStage_TASK_STAGE_ERROR)
	taskStatus_FakeQueuePass                      = core.NewTaskStatus(contract.TaskCategory_TASK_CATEGORY_FAILED, "Fake queue pass", contract.TaskStage_TASK_STAGE_ERROR)
	taskStatusFailedToPassQueue                   = core.NewTaskStatus(contract.TaskCategory_TASK_CATEGORY_FAILED, "Failed to pass queue", contract.TaskStage_TASK_STAGE_ERROR)
)


func taskStatus_FailedSendSensorWithStatus(status int)  *contract.TaskStatusData {
  return core.NewTaskStatus(contract.TaskCategory_TASK_CATEGORY_FAILED, "Failed to send sensor: " + strconv.Itoa(status), contract.TaskStage_TASK_STAGE_ERROR)
}