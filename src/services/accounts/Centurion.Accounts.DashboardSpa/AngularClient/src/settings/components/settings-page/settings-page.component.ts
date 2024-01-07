import {ChangeDetectionStrategy, Component, OnInit} from "@angular/core";
import {MessageService} from "primeng/api";
import {ReleaseKeysDataSource} from "../../../releases/services/release-keys.data-source";
import {DisposableComponentBase} from "../../../shared/components/disposable.component-base";
import {
  DashboardData,
  DashboardHostingMode,
  DashboardsService, StripeWebHooksService,
  TimeZoneData,
  TimeZonesService,
} from "../../../dashboards-api";
import {map} from "rxjs/operators";
import {KeyValuePair} from "../../../core/models/key-value-pair.model";
import {FormUtil} from "../../../core/services/form.util";
import {BehaviorSubject, Subject} from "rxjs";
import {NotificationService} from "../../../core/services/notifications/notification.service";
import {OperationStatusMessage} from "../../../core/services/notifications/messages.model";
import {DashboardFormGroup} from "../../models/dashboard.form-group";
import {ToolbarService} from "../../../core/services/toolbar.service";
import {IdentityService} from "../../../core/services/identity.service";
import {environment} from "../../../environments/environment";
import {ClipboardService} from "../../../core/services/clipboard.service";


@Component({
  selector: "app-settings-page",
  templateUrl: "./settings-page.component.html",
  styleUrls: ["./settings-page.component.less"],
  changeDetection: ChangeDetectionStrategy.OnPush,
  providers: [
    MessageService,
    ReleaseKeysDataSource
  ],
  host: {
    class: "SettingsPage Page"
  }
})
export class SettingsPageComponent extends DisposableComponentBase implements OnInit {
  form = new DashboardFormGroup();

  hostingModes: KeyValuePair<string, DashboardHostingMode>[] = [
    {key: "Path segment", value: DashboardHostingMode.PathSegment},
    {key: "Sub domain", value: DashboardHostingMode.Subdomain},
    {key: "Dedicated domain", value: DashboardHostingMode.Dedicated},
  ];

  timeZones$ = new BehaviorSubject<TimeZoneData[]>([]);

  invalidationToken$ = new Subject<void>();
  stripeWebhookUrl$ = new BehaviorSubject<string>(null);

  constructor(private dashboardsService: DashboardsService,
              private timezonesService: TimeZonesService,
              private toolbarService: ToolbarService,
              private clipboard: ClipboardService,
              private webhookService: StripeWebHooksService,
              private notificationService: NotificationService) {
    super();
    toolbarService.setTitle("Dashboard Setting");
  }

  async ngOnInit(): Promise<void> {
    await this.refreshData();
    const timeZones = await this.asyncTracker.executeAsAsync(
      this.timezonesService.timeZonesGetSupportedTimeZones().pipe(map(_ => _.payload)));
    this.timeZones$.next(timeZones);


    try {
      const stripeWebhookUrl = await this.asyncTracker.executeAsAsync(
        this.webhookService.stripeWebHooksGetWebhookEndpoint().pipe(map(_ => _.payload)));

      this.stripeWebhookUrl$.next(stripeWebhookUrl);
    } catch (e) {
      // noop
    }
  }

  async save(): Promise<void> {
    if (this.form.invalid) {
      FormUtil.validateAllFormFields(this.form);
      return;
    }
    try {
      const data: DashboardData = this.form.value;
      await this.asyncTracker.executeAsAsync(
        this.dashboardsService.dashboardsUpdate(data)
      );

      await this.refreshData();
      this.notificationService.success(OperationStatusMessage.UPDATED);
    } catch (e) {
      this.notificationService.error(OperationStatusMessage.FAILED);
    }
  }

  resetSettings(): void {
    this.form.reset({});
    this.invalidationToken$.next();
  }

  private async refreshData(): Promise<void> {
    const data = await this.asyncTracker.executeAsAsync(
      this.dashboardsService.dashboardsGetOwn().pipe(map(_ => _.payload))
    );

    this.resetSettings();
    this.form.patchValue(data, {emitEvent: false});
  }

  async copyStripeWebhookUrlToClipboard(): Promise<void> {
    await this.clipboard.writeText(this.stripeWebhookUrl$.value);
    this.notificationService.success("Stripe webhook URL copied to clipboard");
  }
}
