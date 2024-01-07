import {
  ChangeDetectionStrategy,
  Component,
  EventEmitter,
  Input,
  OnChanges,
  OnInit,
  Output,
  SimpleChanges
} from "@angular/core";
import {DisposableComponentBase} from "../disposable.component-base";
import {FormControl, Validators} from "@angular/forms";
import {LicenseKeysService} from "../../../dashboards-api";
import {NotificationService} from "../../../core/services/notifications/notification.service";
import {OperationStatusMessage} from "../../../core/services/notifications/messages.model";

@Component({
  selector: "app-bind-license-key-dialog",
  templateUrl: "./bind-license-key-dialog.component.html",
  styleUrls: ["./bind-license-key-dialog.component.less"],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class BindLicenseKeyDialogComponent extends DisposableComponentBase implements OnInit, OnChanges {
  @Input() isVisible = false;
  @Output() isVisibleChange = new EventEmitter<boolean>();
  @Output() bound = new EventEmitter<void>();

  licenseKeyCtrl = new FormControl(null, [Validators.required]);

  constructor(private licenseKeysService: LicenseKeysService,
              private notifications: NotificationService) {
    super();
  }

  ngOnChanges(changes: SimpleChanges): void {
    this.licenseKeyCtrl.reset();
  }

  ngOnInit(): void {
    this.licenseKeyCtrl.reset();
  }

  async bind(): Promise<void> {
    if (this.licenseKeyCtrl.invalid) {
      this.licenseKeyCtrl.markAsDirty();
      this.licenseKeyCtrl.markAllAsTouched();
      this.licenseKeyCtrl.updateValueAndValidity();
      return;
    }

    try {
      await this.asyncTracker.executeAsAsync(this.licenseKeysService.licenseKeysBindToUser(this.licenseKeyCtrl.value));
      this.notifications.success(OperationStatusMessage.DONE);
      this.isVisibleChange.emit(false);
      this.bound.emit();
    } catch (e) {
      this.notifications.error(OperationStatusMessage.FAILED);
    }
  }
}
