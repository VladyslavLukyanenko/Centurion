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
import {DisposableComponentBase} from "../../../shared/components/disposable.component-base";
import {MembersDataSource} from "../../../core/data-sources/members.data-source";
import {LicenseKeysService} from "../../../dashboards-api";
import {NotificationService} from "../../../core/services/notifications/notification.service";
import {OperationStatusMessage} from "../../../core/services/notifications/messages.model";

@Component({
  selector: "app-bind-license-key-to-user-dialog",
  templateUrl: "./bind-license-key-to-user-dialog.component.html",
  styleUrls: ["./bind-license-key-to-user-dialog.component.less"],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class BindLicenseKeyToUserDialogComponent extends DisposableComponentBase implements OnInit, OnChanges {
  @Input() isVisible = false;
  @Input() key: string;
  @Input() membersDataSource: MembersDataSource;
  @Output() isVisibleChange = new EventEmitter<boolean>();
  @Output() bound = new EventEmitter<void>();

  selectedMemberId: number = null;

  constructor(private licenseKeysService: LicenseKeysService,
              private notifications: NotificationService) {
    super();
  }

  ngOnChanges(changes: SimpleChanges): void {
    this.selectedMemberId = null;
  }

  ngOnInit(): void {
    this.selectedMemberId = null;
  }

  async bind(): Promise<void> {
    try {
      await this.asyncTracker.executeAsAsync(this.licenseKeysService.licenseKeysBindToUser(this.key, this.selectedMemberId));
      this.notifications.success(OperationStatusMessage.DONE);
      this.isVisibleChange.emit(false);
      this.bound.emit();
    } catch (e) {
      this.notifications.error(OperationStatusMessage.FAILED);
    }
  }
}
