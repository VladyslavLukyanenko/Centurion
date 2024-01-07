package core

import (
  contract "github.com/CenturionLabs/centurion/checkout-service/contracts"
  integration2 "github.com/CenturionLabs/centurion/checkout-service/contracts/checkout/integration"
  "github.com/CenturionLabs/centurion/checkout-service/contracts/common"
  "google.golang.org/protobuf/types/known/timestamppb"
  "time"
)

func NewTaskStatus(category contract.TaskCategory, title string, stage contract.TaskStage) *contract.TaskStatusData {
	return &contract.TaskStatusData{
		Category: category,
		Title:    title,
		Stage:    stage,
	}
}
func NewTaskStatusWithDefaults(category contract.TaskCategory, title string) *contract.TaskStatusData {
	return &contract.TaskStatusData{
		Category: category,
		Title:    title,
	}
}

func NewTaskStatusWithDesc(category contract.TaskCategory, title string, stage contract.TaskStage, desc string) *contract.TaskStatusData {
	status := NewTaskStatus(category, title, stage)
	status.Description = &desc

	return status
}

func IsTaskCompleted(s *contract.TaskStatusData) bool {
	return s.Stage == contract.TaskStage_TASK_STAGE_IDLE
}

func CreateTerminatedCheckoutStatus(taskId string, userId string) *integration2.CheckoutStatusChanged {
  status := &contract.TaskStatusData{
    Title:    "Terminated",
    Category: contract.TaskCategory_TASK_CATEGORY_TERMINATED,
    Stage: contract.TaskStage_TASK_STAGE_IDLE,
  }
  event := &integration2.CheckoutStatusChanged{
    Status: status,
    Meta: &common.EventMetadata{
      Timestamp: timestamppb.New(time.Now().UTC()),
      TaskId:    taskId,
      UserId:    &userId,
    },
  }
  return event
}
