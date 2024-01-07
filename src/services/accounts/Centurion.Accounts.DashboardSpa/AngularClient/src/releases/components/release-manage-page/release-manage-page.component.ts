import {Component, OnInit, ChangeDetectionStrategy} from "@angular/core";
import {ConfirmationService, MessageService} from "primeng/api";
import {DisposableComponentBase} from "../../../shared/components/disposable.component-base";
import {
  LicenseKeysService,
  PlanData,
  PlansService,
  ReleaseData,
  ReleasesService,
  ReleaseType
} from "../../../dashboards-api";
import {ActivatedRoute, Router} from "@angular/router";
import {BehaviorSubject} from "rxjs";
import {debounceTime, map} from "rxjs/operators";
import {KeyValuePair} from "../../../core/models/key-value-pair.model";
import {ReleaseFormGroup} from "../../models/release.form-group";
import {ReleaseKeysDataSource} from "../../services/release-keys.data-source";
import {ToolbarService} from "../../../core/services/toolbar.service";
import {AppPermissions} from "../../../core/models/permissions.model";

@Component({
  selector: "app-release-manage-page",
  templateUrl: "./release-manage-page.component.html",
  styleUrls: ["./release-manage-page.component.less"],
  changeDetection: ChangeDetectionStrategy.OnPush,
  providers: [
    MessageService,
    ReleaseKeysDataSource
  ],
  host: {
    class: "ReleaseManage Page"
  }
})
export class ReleaseManagePageComponent extends DisposableComponentBase implements OnInit {
  data$ = new BehaviorSubject<ReleaseData>(null);

  releaseTypes: KeyValuePair<string, ReleaseType>[] = [
    {key: ReleaseType.Fcfs, value: ReleaseType.Fcfs}
  ];

  form = new ReleaseFormGroup();
  plans$ = new BehaviorSubject<PlanData[]>([]);

  isDeleteConfirmVisible = false;

  appPermissions = AppPermissions;
  constructor(private licenseKeysServices: LicenseKeysService,
              private router: Router,
              private releasesService: ReleasesService,
              private plansService: PlansService,
              private messageService: MessageService,
              private confirmService: ConfirmationService,
              private toolbarService: ToolbarService,
              private activatedRoute: ActivatedRoute,
              readonly releaseKeysDataSource: ReleaseKeysDataSource) {
    super();
    toolbarService.setTitle("Release Details");
  }

  async ngOnInit(): Promise<void> {
    this.activatedRoute.params.pipe(this.untilDestroy())
      .subscribe(async params => {
        const id = params.id;

        await this.fetchData(id);
      });

    this.form.valueChanges
      .pipe(
        this.untilDestroy(),
        debounceTime(500)
      )
      .subscribe(async cmd => {
        try {
          const id = this.data$.value.id;
          await this.asyncTracker.executeAsAsync(this.releasesService.releasesUpdate(id, cmd));
          await this.fetchData(id);
          this.messageService.add({key: "notifications", severity: "success", detail: "Changes saved", life: 1000});
        } catch (e) {
          this.messageService.add({
            key: "notifications",
            severity: "error",
            detail: e.error?.error?.message || "An error occurred on release update",
            life: 3000
          });
        }
      });

    const plans = await this.asyncTracker.executeAsAsync(
      this.plansService.plansGetAll().pipe(map(_ => _.payload))
    );

    this.plans$.next(plans);
  }

  async delete(): Promise<void> {
    try {
      await this.asyncTracker.executeAsAsync(this.releasesService.releasesRemove(this.data$.value.id));
      this.messageService.add({
        key: "notifications",
        severity: "success",
        detail: "Release was deleted successfully",
        life: 1000
      });

      await this.router.navigate([".."], {relativeTo: this.activatedRoute});
    } catch (e) {
      this.messageService.add({
        key: "notifications",
        severity: "error",
        detail: e.error?.error?.message || "An error occurred on release deletion",
        life: 3000
      });
    }
  }

  async toggleActive(): Promise<void> {
    try {
      const shouldBeActive = !this.data$.value.isActive;
      const id = this.data$.value.id;
      await this.asyncTracker.executeAsAsync(this.releasesService.releasesToggleActive(id, shouldBeActive));
      await this.fetchData(id);
      this.messageService.add({
        key: "notifications",
        severity: "success",
        detail: `Release was ${shouldBeActive ? "activated" : "deactivated"} successfully`,
        life: 1000
      });
    } catch (e) {
      this.messageService.add({
        key: "notifications",
        severity: "error",
        detail: e.error?.error?.message || "An error occurred on release update",
        life: 3000
      });
    }
  }

  private async fetchData(id: number): Promise<void> {
    const r = await this.asyncTracker.executeAsAsync(
      this.releasesService.releasesGetById(id).pipe(map(_ => _.payload))
    );

    this.data$.next(r);
    this.releaseKeysDataSource.setReleaseId(id);
    this.form.patchValue(r, {emitEvent: false});
  }
}
