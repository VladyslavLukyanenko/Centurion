package fake_shop

import (
  "github.com/CenturionLabs/centurion/checkout-service/contracts"
  "github.com/CenturionLabs/centurion/checkout-service/core"
)

var (
  taskStatus__Monitoring = core.NewTaskStatus(contracts.TaskCategory_TASK_CATEGORY_UNSPECIFIED, "Monitoring", contracts.TaskStage_TASK_STAGE_RUNNING)

  taskStatus__SessionInitializing = core.NewTaskStatus(contracts.TaskCategory_TASK_CATEGORY_UNSPECIFIED, "Session Initializing", contracts.TaskStage_TASK_STAGE_STARTING)
  taskStatus__SessionInitialized  = core.NewTaskStatus(contracts.TaskCategory_TASK_CATEGORY_UNSPECIFIED, "Session Initialized", contracts.TaskStage_TASK_STAGE_RUNNING)
  taskStatus__FailedToSessionInitialize  = core.NewTaskStatus(contracts.TaskCategory_TASK_CATEGORY_FAILED, "Failed To Initialize Session", contracts.TaskStage_TASK_STAGE_ERROR)

  taskStatus__SolvingProtection = core.NewTaskStatus(contracts.TaskCategory_TASK_CATEGORY_UNSPECIFIED, "Solving Protection", contracts.TaskStage_TASK_STAGE_RUNNING)
  taskStatus__SolvedProtection  = core.NewTaskStatus(contracts.TaskCategory_TASK_CATEGORY_UNSPECIFIED, "Solved Protection", contracts.TaskStage_TASK_STAGE_RUNNING)
  taskStatus__FailedToSolveProtection  = core.NewTaskStatus(contracts.TaskCategory_TASK_CATEGORY_FAILED, "Failed To Solve Protection", contracts.TaskStage_TASK_STAGE_ERROR)

  taskStatus__CartingProduct = core.NewTaskStatus(contracts.TaskCategory_TASK_CATEGORY_UNSPECIFIED, "Carting Product", contracts.TaskStage_TASK_STAGE_CHECKING_OUT)
  taskStatus__CartedProduct  = core.NewTaskStatus(contracts.TaskCategory_TASK_CATEGORY_CARTED, "Carted Product", contracts.TaskStage_TASK_STAGE_CHECKING_OUT)
  taskStatus__FailedToCartProduct  = core.NewTaskStatus(contracts.TaskCategory_TASK_CATEGORY_FAILED, "Failed To Cart Product", contracts.TaskStage_TASK_STAGE_ERROR)

  taskStatus__PlacingOrder = core.NewTaskStatus(contracts.TaskCategory_TASK_CATEGORY_CARTED, "Placing Order", contracts.TaskStage_TASK_STAGE_CHECKING_OUT)
  taskStatus__PlacedOrder  = core.NewTaskStatus(contracts.TaskCategory_TASK_CATEGORY_CARTED, "Placed Order", contracts.TaskStage_TASK_STAGE_CHECKING_OUT)
  taskStatus__FailedToPlaceOrder  = core.NewTaskStatus(contracts.TaskCategory_TASK_CATEGORY_DECLINED, "Failed To Place Order", contracts.TaskStage_TASK_STAGE_ERROR)

  taskStatus__Purchasing = core.NewTaskStatus(contracts.TaskCategory_TASK_CATEGORY_CARTED, "Purchasing", contracts.TaskStage_TASK_STAGE_CHECKING_OUT)
  taskStatus__FailedToPurchase = core.NewTaskStatus(contracts.TaskCategory_TASK_CATEGORY_DECLINED, "Failed To Purchase", contracts.TaskStage_TASK_STAGE_ERROR)
)
