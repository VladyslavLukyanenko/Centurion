import {ChangeDetectionStrategy, Component, OnInit} from "@angular/core";
import {DisposableComponentBase} from "../../../shared/components/disposable.component-base";
import {
  LicenseKeyShortData,
  LicenseKeySnapshotData,
  LicenseKeysService,
  PaymentsService, PurchasedLicenseKeyData
} from "../../../dashboards-api";
import {BehaviorSubject, combineLatest} from "rxjs";
import {Observable} from "rxjs/internal/Observable";
import {map} from "rxjs/operators";
import {NotificationService} from "../../../core/services/notifications/notification.service";
import {OperationStatusMessage} from "../../../core/services/notifications/messages.model";
import {ToolbarService} from "../../../core/services/toolbar.service";



@Component({
  selector: "app-purchases-page",
  templateUrl: "./purchases-page.component.html",
  styleUrls: ["./purchases-page.component.less"],
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: {
    class: "Page PurchasesPage"
  }
})
export class PurchasesPageComponent extends DisposableComponentBase implements OnInit {
  licenseKeys$ = new BehaviorSubject<LicenseKeyShortData[]>([]);
  billingPortalUrl$ = new BehaviorSubject<string>(null);
  noData$: Observable<boolean>;
  isBindDialogVisible$ = new BehaviorSubject(false);
  unbindLicenseKey$ = new BehaviorSubject<string>(null);

  constructor(private licenseKeyService: LicenseKeysService,
              private paymentService: PaymentsService,
              toolbarService: ToolbarService,
              private notifications: NotificationService) {
    super();
    toolbarService.setTitle("Purchased Keys");
  }

  async ngOnInit(): Promise<void> {
    this.noData$ = combineLatest([this.asyncTracker.isLoading$, this.licenseKeys$])
      .pipe(map(([isLoading, keys]) => !isLoading && !keys.length));

    await this.refreshData();

    try {
      const url = await this.asyncTracker.executeAsAsync(
        this.paymentService.paymentsGetBillingPortalLink()
      );

      this.billingPortalUrl$.next(url.payload);
    } catch (e) {
      // noop
    }
  }

  formatDate(rawDate: string): string {
    if (!rawDate) {
      return null;
    }

    const end = rawDate.indexOf("T");
    return rawDate.substring(0, end);
  }

  trackById = (_: number, r: PurchasedLicenseKeyData) => r?.id;

  async refreshData(): Promise<void> {
    const items = await this.asyncTracker.executeAsAsync(
      this.licenseKeyService.licenseKeysGetPurchasedLicenseKeys()
    );

    this.licenseKeys$.next(items.payload);
  }

  openBillingPortal(): void {
    if (!this.billingPortalUrl$.value) {
      return;
    }

    window.open(this.billingPortalUrl$.value, "_blank");
  }

  async resetSession(key: string): Promise<void> {
    try {
      await this.asyncTracker.executeAsAsync(this.licenseKeyService.licenseKeysReset(key));
      this.notifications.success(OperationStatusMessage.DONE);
    } catch (e) {
      this.notifications.error(OperationStatusMessage.FAILED);
    }
  }

  isDeactivated(l: PurchasedLicenseKeyData): boolean {
    return l.isExpired || l.isSubscriptionCancelled;
  }
}
