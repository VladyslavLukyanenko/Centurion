import {Injectable} from "@angular/core";
import {OperationStatusMessage} from "./messages.model";
import {MessageService} from "primeng/api";
import {Message} from "primeng/api/message";

const snackBarConfig = {
  error: {
    key: "notifications",
    severity: "error",
    life: 5000
  } as Message,
  warn: {
    key: "notifications",
    severity: "warning",
    life: 3000
  } as Message,
  success: {
    key: "notifications",
    severity: "success",
    life: 1000
  } as Message,
  info: {
    key: "notifications",
    severity: "information",
    life: 1000
  } as Message
};

@Injectable()
export class NotificationService {

  constructor(
    private readonly messageService: MessageService
  ) {
  }

  error(message: string | OperationStatusMessage): void {
    this.displayPlainOrOpMessage(message, snackBarConfig.error);
  }

  warn(message: string | OperationStatusMessage): void {
    this.displayPlainOrOpMessage(message, snackBarConfig.warn);
  }

  success(message: string | OperationStatusMessage): void {
    this.displayPlainOrOpMessage(message, snackBarConfig.success);
  }

  info(message: string | OperationStatusMessage): void {
    this.displayPlainOrOpMessage(message, snackBarConfig.info);
  }

  private displayPlainOrOpMessage(message: string | any, cfg: Message): void {
    const displayMessage = {
      ...cfg,
    };

    if (message instanceof OperationStatusMessage) {
      displayMessage.detail = message.messageKey;
    } else {
      displayMessage.detail = message;
    }
    this.messageService.add(displayMessage);
  }
}
