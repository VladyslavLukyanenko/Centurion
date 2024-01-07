package amazon

import (
	contract "github.com/CenturionLabs/centurion/checkout-service/contracts"
	"github.com/CenturionLabs/centurion/checkout-service/core"
	"net/url"
	"strconv"
)

var (
	taskStatus_ValidateAccInProgress        = core.NewTaskStatus(contract.TaskCategory_TASK_CATEGORY_UNSPECIFIED, "Checking Login", contract.TaskStage_TASK_STAGE_STARTING)
	taskStatus_ValidateAccWaitingForPending = core.NewTaskStatus(contract.TaskCategory_TASK_CATEGORY_UNSPECIFIED, "Waiting For Check Finish", contract.TaskStage_TASK_STAGE_STARTING)

	taskStatus_FetchWebLabGetToken          = core.NewTaskStatus(contract.TaskCategory_TASK_CATEGORY_UNSPECIFIED, "Getting Account Token", contract.TaskStage_TASK_STAGE_RUNNING)
	taskStatus_FetchWebLabWaitingForPending = core.NewTaskStatus(contract.TaskCategory_TASK_CATEGORY_UNSPECIFIED, "Waiting For Account Token Finish", contract.TaskStage_TASK_STAGE_RUNNING)

	taskStatus_SessionLoaded         = core.NewTaskStatus(contract.TaskCategory_TASK_CATEGORY_UNSPECIFIED, "Loaded Session", contract.TaskStage_TASK_STAGE_RUNNING)
	taskStatus_FailedSessionLoad     = core.NewTaskStatus(contract.TaskCategory_TASK_CATEGORY_FATAL_ERROR, "Failed To Load Session", contract.TaskStage_TASK_STAGE_IDLE)
	taskStatus_SessionHasNoCookies   = core.NewTaskStatus(contract.TaskCategory_TASK_CATEGORY_FATAL_ERROR, "Session Contains No Cookies", contract.TaskStage_TASK_STAGE_IDLE)
	taskStatus_ValidateAccLoginValid = core.NewTaskStatus(contract.TaskCategory_TASK_CATEGORY_UNSPECIFIED, "Login Valid", contract.TaskStage_TASK_STAGE_RUNNING)
	taskStatus_FetchWebLabFetched    = core.NewTaskStatus(contract.TaskCategory_TASK_CATEGORY_UNSPECIFIED, "Fetched WebLab", contract.TaskStage_TASK_STAGE_RUNNING)
	taskStatus_PlaceOrderPlacing     = core.NewTaskStatus(contract.TaskCategory_TASK_CATEGORY_UNSPECIFIED, "Placing Order", contract.TaskStage_TASK_STAGE_CHECKING_OUT)
	taskStatus_PlaceOrderPlaced      = core.NewTaskStatus(contract.TaskCategory_TASK_CATEGORY_CARTED, "Order Placed", contract.TaskStage_TASK_STAGE_CHECKING_OUT)
	taskStatus_PlaceOrderOOS         = core.NewTaskStatus(contract.TaskCategory_TASK_CATEGORY_FATAL_ERROR, "Out Of Stock", contract.TaskStage_TASK_STAGE_IDLE)

	taskStatus_SessionNotFound    = core.NewTaskStatus(contract.TaskCategory_TASK_CATEGORY_FATAL_ERROR, "Session Not Found", contract.TaskStage_TASK_STAGE_IDLE)
	taskStatus_SessionIsNotReady  = core.NewTaskStatus(contract.TaskCategory_TASK_CATEGORY_FATAL_ERROR, "Session Is Not Ready", contract.TaskStage_TASK_STAGE_IDLE)
	taskStatus_SessionNotSelected = core.NewTaskStatus(contract.TaskCategory_TASK_CATEGORY_FATAL_ERROR, "Session Is Not Selected", contract.TaskStage_TASK_STAGE_IDLE)

	taskStatus_InvalidSettings       = core.NewTaskStatus(contract.TaskCategory_TASK_CATEGORY_FAILED, "Invalid Settings / Antibot - Rotating Proxy", contract.TaskStage_TASK_STAGE_ERROR)
	taskStatus_ProxyBanned           = core.NewTaskStatus(contract.TaskCategory_TASK_CATEGORY_FAILED, "Proxy Banned | Rotating Proxy", contract.TaskStage_TASK_STAGE_ERROR)
	taskStatus_UnknownErrorMonitor   = core.NewTaskStatus(contract.TaskCategory_TASK_CATEGORY_FAILED, "Unknown Error Monitoring", contract.TaskStage_TASK_STAGE_ERROR)
	taskStatus_MonitorOutOfStock     = core.NewTaskStatus(contract.TaskCategory_TASK_CATEGORY_FAILED, "Out Of Stock", contract.TaskStage_TASK_STAGE_ERROR)
	taskStatus_MonitorInStock        = core.NewTaskStatus(contract.TaskCategory_TASK_CATEGORY_UNSPECIFIED, "In Stock", contract.TaskStage_TASK_STAGE_RUNNING)
	taskStatus_Monitoring            = core.NewTaskStatus(contract.TaskCategory_TASK_CATEGORY_UNSPECIFIED, "Monitoring", contract.TaskStage_TASK_STAGE_RUNNING)
	taskStatus_UnknownCartingError   = core.NewTaskStatus(contract.TaskCategory_TASK_CATEGORY_FAILED, "Unknown Carting Error", contract.TaskStage_TASK_STAGE_ERROR)
	taskStatus_ItemNotAdded          = core.NewTaskStatus(contract.TaskCategory_TASK_CATEGORY_FAILED, "Item Not Added", contract.TaskStage_TASK_STAGE_ERROR)
	taskStatus_AddedToCart           = core.NewTaskStatus(contract.TaskCategory_TASK_CATEGORY_CARTED, "Added To Cart", contract.TaskStage_TASK_STAGE_CHECKING_OUT)
	taskStatus_FailedToFetchCheckout = core.NewTaskStatus(contract.TaskCategory_TASK_CATEGORY_FAILED, "Failed To Fetch Checkout", contract.TaskStage_TASK_STAGE_ERROR)

	taskStatus_ValidateAccSessionInvalid          = core.NewTaskStatus(contract.TaskCategory_TASK_CATEGORY_FATAL_ERROR, "Session Invalid - Relogin", contract.TaskStage_TASK_STAGE_IDLE)
	taskStatus_ValidateAccReverifyAccountSettings = core.NewTaskStatus(contract.TaskCategory_TASK_CATEGORY_FATAL_ERROR, "Reverify Account Settings", contract.TaskStage_TASK_STAGE_IDLE)
	taskStatus_AmazonAntiBot                      = core.NewTaskStatus(contract.TaskCategory_TASK_CATEGORY_FAILED, "Amazon AntiBot | Rotating Proxy", contract.TaskStage_TASK_STAGE_ERROR)
	taskStatus_CaptchaDetected                    = core.NewTaskStatus(contract.TaskCategory_TASK_CATEGORY_FAILED, "Captcha Detected | Rotating Proxy", contract.TaskStage_TASK_STAGE_ERROR)
	taskStatus_FailedToFetchOrderStatus           = core.NewTaskStatus(contract.TaskCategory_TASK_CATEGORY_FATAL_ERROR, "Failed To Fetch Order Status", contract.TaskStage_TASK_STAGE_IDLE)
	taskStatus_PotentialAccountClip               = core.NewTaskStatus(contract.TaskCategory_TASK_CATEGORY_FAILED, "Potential Account Clip / Bad Cookies - Rotating Proxy", contract.TaskStage_TASK_STAGE_ERROR)
	taskStatus_FetchWebLabCaptchaDetected         = core.NewTaskStatus(contract.TaskCategory_TASK_CATEGORY_FAILED, "Captcha Detected | Rotating Proxy", contract.TaskStage_TASK_STAGE_ERROR)
	taskStatus_PlaceOrderCheckoutExpired          = core.NewTaskStatus(contract.TaskCategory_TASK_CATEGORY_FATAL_ERROR, "Checkout Expired", contract.TaskStage_TASK_STAGE_IDLE)
)

func taskStatus_ValidateAccFailedToValidate(statusCode int) *contract.TaskStatusData {
	return core.NewTaskStatus(contract.TaskCategory_TASK_CATEGORY_FATAL_ERROR, "Failed To Validate Login - "+strconv.Itoa(statusCode), contract.TaskStage_TASK_STAGE_IDLE)
}

func taskStatus_UnknownHttpErrorMonitor(statusCode int) *contract.TaskStatusData {
	return core.NewTaskStatus(contract.TaskCategory_TASK_CATEGORY_FAILED, "Unknown Monitor Error - "+strconv.Itoa(statusCode), contract.TaskStage_TASK_STAGE_ERROR)
}

func taskStatus_FetchWebLabFailedToFetch(statusCode int) *contract.TaskStatusData {
	return core.NewTaskStatus(contract.TaskCategory_TASK_CATEGORY_FATAL_ERROR, "Error Getting Account Token - "+strconv.Itoa(statusCode), contract.TaskStage_TASK_STAGE_IDLE)
}

func taskStatus_PlaceOrderFailedToPlaceOrder(statusCode int) *contract.TaskStatusData {
	return core.NewTaskStatus(contract.TaskCategory_TASK_CATEGORY_FATAL_ERROR, "Failed To Place Order - "+strconv.Itoa(statusCode), contract.TaskStage_TASK_STAGE_IDLE)
}

func taskStatus_PlaceOrderUnknownError(url *url.URL) *contract.TaskStatusData {
	return core.NewTaskStatus(contract.TaskCategory_TASK_CATEGORY_FATAL_ERROR, "Unknown Order Error - "+url.String(), contract.TaskStage_TASK_STAGE_IDLE)
}

func taskStatus_FailedToATCError(statusCode int) *contract.TaskStatusData {
	return core.NewTaskStatus(contract.TaskCategory_TASK_CATEGORY_FATAL_ERROR, "Failed To ATC - "+strconv.Itoa(statusCode), contract.TaskStage_TASK_STAGE_IDLE)
}
