import {ChangeDetectionStrategy, Component, OnInit} from "@angular/core";
import {LicenseKeysService, LicenseKeySummaryData, PlanData, PlansService} from "../../../dashboards-api";
import {DisposableComponentBase} from "../../../shared/components/disposable.component-base";
import {BehaviorSubject} from "rxjs";
import {ActivatedRoute, Router} from "@angular/router";
import {DateUtil} from "../../../core/services/date.util";
import {KeyValuePair} from "../../../core/models/key-value-pair.model";
import {debounceTime, map} from "rxjs/operators";
import {FormControl, FormGroup} from "@angular/forms";
import {ConfirmationService} from "primeng/api";
import {ToolbarService} from "../../../core/services/toolbar.service";
import {AppPermissions} from "../../../core/models/permissions.model";
import {AuthorizePermissions} from "../../../core/decorators/permission.decorator";
import moment from "moment";
import {MembersDataSource} from "../../../core/data-sources/members.data-source";
import {NotificationService} from "../../../core/services/notifications/notification.service";

@Component({
  selector: "app-license-manage-page",
  templateUrl: "./license-manage-page.component.html",
  styleUrls: ["./license-manage-page.component.less"],
  changeDetection: ChangeDetectionStrategy.OnPush,
  providers: [MembersDataSource],
  host: {
    class: "Licenses Page"
  }
})
@AuthorizePermissions(AppPermissions.LicenseKeysManage)
export class LicenseManagePageComponent extends DisposableComponentBase implements OnInit {
  data$ = new BehaviorSubject<LicenseKeySummaryData>(null);

  keyTypes: KeyValuePair<string, boolean>[] = [
    {key: "Renewal", value: false},
    {key: "Lifetime", value: true},
  ];

  plans$ = new BehaviorSubject<PlanData[]>([]);

  form = new LicenseKeyFormGroup();

  isAddExtraDaysVisible$ = new BehaviorSubject(false);
  isBindDialogVisible$ = new BehaviorSubject(false);

  appPermissions = AppPermissions;

  constructor(private licenseKeysServices: LicenseKeysService,
              private router: Router,
              private plansService: PlansService,
              private confirmService: ConfirmationService,
              private toolbarService: ToolbarService,
              readonly membersDataSource: MembersDataSource,
              private notifications: NotificationService,
              private activatedRoute: ActivatedRoute) {
    super();
    toolbarService.setTitle("License Key Details");
    membersDataSource.setIncludeStaff(true);
  }

  async ngOnInit(): Promise<any> {
    this.activatedRoute.params
      .pipe(
        this.untilDestroy()
      )
      .subscribe(async params => {
        const id = params.id;
        await this.refreshData(id);
      });

    this.form.valueChanges
      .pipe(
        this.untilDestroy(),
        debounceTime(500)
      )
      .subscribe(async cmd => {
        try {
          const id = this.data$.value.id;
          await this.asyncTracker.executeAsAsync(
            this.licenseKeysServices.licenseKeysUpdateLicenseKey(id, cmd));
          await this.refreshData(id);
          this.notifications.success("Changes saved");
        } catch (e) {
          this.notifications.error(e.error?.error?.message || "An error occurred on license key update");
        }
      });

    const plans = await this.asyncTracker.executeAsAsync(
      this.plansService.plansGetAll().pipe(map(_ => _.payload))
    );

    this.plans$.next(plans);
  }

  async refreshData(id?: number): Promise<any> {
    id = id || this.data$.value?.id;

    const r = await this.asyncTracker.executeAsAsync(
      this.licenseKeysServices.licenseKeysGetLicenseKeySummary(id));
    this.data$.next(r.payload);
    this.form.patchValue(r.payload, {emitEvent: false});
  }

  getFormattedDate(joinedAt: string): string {
    return DateUtil.humanizeDate(joinedAt);
  }

  terminate(e: Event): void {
    this.confirmService.confirm({
      message: "Are you sure to terminate this key?",
      target: e.target,
      icon: "pi pi-exclamation-triangle",
      accept: async () => {
        try {
          await this.asyncTracker.executeAsAsync(this.licenseKeysServices.licenseKeysRemove(this.data$.value.id));
          this.notifications.success("License key terminated successfully");
          await this.router.navigate([".."], {relativeTo: this.activatedRoute});
        } catch (e) {
          this.notifications.error(e.error?.error?.message || "An error occurred on license key termination");
        }
      }
    });
  }

  unbind(e: Event): void {
    this.confirmService.confirm({
      message: "Are you sure to unbind this key?",
      target: e.target,
      icon: "pi pi-exclamation-triangle",
      accept: async () => {
        try {
          await this.asyncTracker.executeAsAsync(
            this.licenseKeysServices.licenseKeysUnbindFromUser(this.data$.value.value));

          this.notifications.success("License key unbound successfully");
          await this.router.navigate([".."], {relativeTo: this.activatedRoute});
        } catch (e) {
          this.notifications.error(e.error?.error?.message || "An error occurred on license key unbound");
        }
      }
    });
  }

  unbindableAfterTextVisible(): boolean {
    return moment(this.data$.value.unbindableAfter).isAfter(moment());
  }
}


export class LicenseKeyFormGroup extends FormGroup {
  constructor(data?: LicenseKeySummaryData) {
    super({
      id: new FormControl(data?.id),
      planId: new FormControl(data?.planId),
      isLifetime: new FormControl(!data?.expiry),
      notes: new FormControl(data?.reason),
      isUnbindable: new FormControl(!!data?.unbindableAfter),
    });
  }

  patchValue(value: { [p: string]: any }, options?: { onlySelf?: boolean; emitEvent?: boolean }): void {
    super.patchValue({
      id: value?.id,
      planId: value?.planId,
      isLifetime: !value?.expiry,
      notes: value?.reason,
      isUnbindable: !!value?.unbindableAfter,
    }, options);
  }

  get isUnbindable(): boolean {
    return this.get("isUnbindable").value;
  }
}
