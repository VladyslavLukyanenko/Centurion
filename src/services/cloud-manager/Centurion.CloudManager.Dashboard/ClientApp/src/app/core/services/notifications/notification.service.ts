import {Injectable} from "@angular/core";
import {TranslateService} from "@ngx-translate/core";
import {OperationStatusMessage} from "./messages.model";
import {NzMessageDataOptions, NzMessageService, NzMessageType} from "ng-zorro-antd";

interface NotificationMsgCfg {
  type: NzMessageType,
  options: NzMessageDataOptions
}

const cfg = {
  error: {
    type: "error",
    options: {
      nzDuration: 5000,
      nzPauseOnHover: true
    }
  } as NotificationMsgCfg,
  warn: {
    type: "warning",
    options: {
      nzDuration: 3000,
      nzPauseOnHover: true
    }
  } as NotificationMsgCfg,
  success: {
    type: "success",
    options: {
      nzDuration: 1500,
      nzPauseOnHover: true
    }
  } as NotificationMsgCfg
};

@Injectable({
  providedIn: "root"
})
export class NotificationService {

  constructor(
    private readonly messageService: NzMessageService,
    private translate: TranslateService
  ) {
  }

  error(message: string | any) {
    this.displayPlainOrOpMessage(message, cfg.error);
  }

  warn(message: string | any) {
    this.displayPlainOrOpMessage(message, cfg.warn);
  }

  success(message: string | any) {
    this.displayPlainOrOpMessage(message, cfg.success, null);
  }

  private displayPlainOrOpMessage(message: string | any, cfg: NotificationMsgCfg, action: string = "Ok") {
    if (message instanceof OperationStatusMessage) {
      this.translate.get(message.messageKey, message.messageArgs)
        .subscribe(m => this.messageService.create(cfg.type, m, cfg.options));
    } else {
      this.messageService.create(cfg.type, message, cfg.options);
    }
  }
}
