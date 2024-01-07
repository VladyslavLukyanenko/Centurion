import {ChangeDetectionStrategy, Component, OnInit} from "@angular/core";
import {
  PlanData,
  PlansService,
  ReleaseData,
  ReleasesService,
  ReleasesSortBy,
  ReleaseType
} from "../../../dashboards-api";
import {DisposableComponentBase} from "../../../shared/components/disposable.component-base";
import {ConfirmationService} from "primeng/api";
import {ReleasesDataSource} from "../../services/releases.data-source";
import {BehaviorSubject} from "rxjs";
import {ActiveFilter} from "../../../core/models/active-filter.model";
import {ActivatedRoute, Router} from "@angular/router";
import {NumsUtil} from "../../../core/services/nums.util";
import {map} from "rxjs/operators";
import {KeyValuePair} from "../../../core/models/key-value-pair.model";
import {ToolbarService} from "../../../core/services/toolbar.service";
import {AppPermissions} from "../../../core/models/permissions.model";
import {AuthorizePermissions} from "../../../core/decorators/permission.decorator";

@Component({
  selector: "app-releases-table-page",
  templateUrl: "./releases-table-page.component.html",
  styleUrls: ["./releases-table-page.component.less"],
  changeDetection: ChangeDetectionStrategy.OnPush,
  providers: [ReleasesDataSource],
  host: {
    class: "Page Releases"
  }
})
@AuthorizePermissions(AppPermissions.ReleaseManage)
export class ReleasesTablePageComponent extends DisposableComponentBase implements OnInit {
  keyTypes = [
    "Renewal",
    "Lifetime"
  ];

  plans$ = new BehaviorSubject<PlanData[]>([]);

  activeFilters$ = new BehaviorSubject<ActiveFilter[]>([]);
  sortKinds = {
    sort: "sort",
    planId: "planId",
    type: "type"
  };

  supportedFilters: ActiveFilter[] = [
    {
      displayName: "newest",
      kind: this.sortKinds.sort,
      label: "Sort by new",
      value: ReleasesSortBy.Newest,
      command: () => this.pushFilterToRoute(this.sortKinds.sort, ReleasesSortBy.Newest)
    },
    {
      displayName: "oldest",
      kind: this.sortKinds.sort,
      label: "Sort by oldest",
      value: ReleasesSortBy.Oldest,
      command: () => this.pushFilterToRoute(this.sortKinds.sort, ReleasesSortBy.Oldest)
    },
    {
      displayName: "plan",
      kind: this.sortKinds.planId,
      label: "Sort by plan",
      value: null,
      command: () => this.isPlanPickerVisible = true
    },
    {
      displayName: "stock",
      kind: this.sortKinds.sort,
      label: "Sort by stock",
      value: ReleasesSortBy.Stock,
      command: () => this.pushFilterToRoute(this.sortKinds.sort, ReleasesSortBy.Stock)
    },
    {
      displayName: "type",
      kind: this.sortKinds.type,
      label: "Sort by type",
      value: null,
      command: () => this.isReleaseTypeVisible = true
    },
  ];

  releaseTypes: KeyValuePair<string, ReleaseType>[] = [
    {key: ReleaseType.Fcfs, value: ReleaseType.Fcfs}
  ];

  isReleaseTypeVisible = false;
  isPlanPickerVisible = false;
  isCreateDialogVisible = false;

  appPermissions = AppPermissions;
  constructor(readonly releasesDataSource: ReleasesDataSource,
              private releasesService: ReleasesService,
              private router: Router,
              private plansService: PlansService,
              private activatedRoute: ActivatedRoute,
              private toolbarService: ToolbarService,
              private confirmationService: ConfirmationService) {
    super();
    toolbarService.setTitle("Releases");
  }

  async ngOnInit(): Promise<any> {
    this.activatedRoute.queryParams
      .pipe(
        this.untilDestroy(),
      )
      .subscribe(async p => {
        const sort: ReleasesSortBy = p.sort;
        const planId = NumsUtil.parseOrNull(p.planId);
        const type = p.type;

        const activeFilters: ActiveFilter[] = [];
        if (sort) {
          activeFilters.push(this.supportedFilters.find(_ => _.kind === this.sortKinds.sort && _.value === sort));
        }

        if (planId) {
          activeFilters.push({
            ...this.supportedFilters.find(_ => _.kind === this.sortKinds.planId),
            value: planId
          });
        }

        if (type != null) {
          activeFilters.push({
            ...this.supportedFilters.find(_ => _.kind === this.sortKinds.type),
            value: type
          });
        }

        this.activeFilters$.next(activeFilters);

        this.releasesDataSource.setPlanId(planId);
        this.releasesDataSource.setType(type);
        this.releasesDataSource.setSortBy(sort);
      });

    const plans = await this.asyncTracker.executeAsAsync(
      this.plansService.plansGetAll().pipe(map(_ => _.payload)));

    this.plans$.next(plans);
  }

  trackById = (_: number, r: ReleaseData) => r?.id;

  remove(r: ReleaseData, e: Event): void {
    this.confirmationService.confirm({
      target: e.target,
      message: `Are you sure to remove "${r.title}" release?`,
      icon: "pi pi-exclamation-triangle",
      accept: async () => {
        await this.asyncTracker.executeAsAsync(this.releasesService.releasesRemove(r.id));
        await this.releasesDataSource.refreshData();
      }
    });
  }

  refresh(): Promise<any> {
    this.isCreateDialogVisible = false;
    return this.releasesDataSource.refreshData();
  }

  pushFilterToRoute(kind: string, value?: any): Promise<any> {
    return this.router.navigate([], {
      queryParamsHandling: "merge",
      relativeTo: this.activatedRoute,
      queryParams: {
        [kind]: value
      }
    });
  }
}
