import {ChangeDetectionStrategy, Component, EventEmitter, Input, Output} from "@angular/core";
import {DisposableComponentBase} from "../../../shared/components/disposable.component-base";
import {LicenseKeysService} from "../../../dashboards-api";
import {NotificationService} from "../../../core/services/notifications/notification.service";
import {OperationStatusMessage} from "../../../core/services/notifications/messages.model";
import {ClipboardService} from "../../../core/services/clipboard.service";
import {BehaviorSubject} from "rxjs";

@Component({
  selector: "app-unbind-license-key-dialog",
  templateUrl: "./unbind-license-key-dialog.component.html",
  styleUrls: ["./unbind-license-key-dialog.component.less"],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class UnbindLicenseKeyDialogComponent extends DisposableComponentBase {
  @Input() isVisible = false;
  @Input() licenseKey: string;
  @Output() isVisibleChange = new EventEmitter<boolean>();
  @Output() unbound = new EventEmitter<void>();

  generatedKey$ = new BehaviorSubject<string>(null);
  constructor(private licenseKeysService: LicenseKeysService,
              private notifications: NotificationService,
              private clipboardService: ClipboardService) {
    super();
  }

  async copyToClipboardAndClose(): Promise<void> {
    try {
      await this.clipboardService.writeText(this.generatedKey$.value);
      this.notifications.success("Key was copied to your clipboard");
      this.isVisibleChange.emit(false);
    } catch (e) {
      this.notifications.error(OperationStatusMessage.FAILED);
    }
  }

  async unbind(): Promise<void> {
    try {
      const unboundGeneratedKey = await this.asyncTracker.executeAsAsync(
        this.licenseKeysService.licenseKeysUnbindFromUser(this.licenseKey));
      this.generatedKey$.next(unboundGeneratedKey.payload);
      this.notifications.success("Key was unbound successfully");
      this.unbound.emit();
    } catch (e) {
      this.notifications.error(OperationStatusMessage.FAILED);
    }
  }

  cancel(): void {
    this.isVisibleChange.next(false);
  }
}
