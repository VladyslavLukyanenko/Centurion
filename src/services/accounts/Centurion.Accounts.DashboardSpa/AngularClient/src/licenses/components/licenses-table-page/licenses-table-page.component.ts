import {ChangeDetectionStrategy, Component, OnInit} from "@angular/core";
import {DisposableComponentBase} from "../../../shared/components/disposable.component-base";
import {
  ActiveReleaseInfoData,
  LicenseKeySnapshotData,
  LicensesSortBy,
  MemberRoleBindingsService,
  MemberSummaryData,
  PlanData,
  PlansService,
  ReleasesService
} from "../../../dashboards-api";
import {BehaviorSubject} from "rxjs";
import {KeyValuePair} from "../../../core/models/key-value-pair.model";
import {ActivatedRoute, Router} from "@angular/router";
import {GlobalShadowService} from "../../../core/services/global-shadow.service";
import {LicensesDataSource} from "../../services/licenses.data-source";
import {map} from "rxjs/operators";
import {NumsUtil} from "../../../core/services/nums.util";
import {ActiveFilter} from "../../../core/models/active-filter.model";
import {ToolbarService} from "../../../core/services/toolbar.service";
import {AuthorizePermissions} from "../../../core/decorators/permission.decorator";
import {AppPermissions} from "../../../core/models/permissions.model";

const types = {
  lifetime: "lifetime",
  renewal: "renewal"
};

const supportedTypes = [
  types.lifetime, types.renewal
];

@Component({
  selector: "app-licenses-table-page",
  templateUrl: "./licenses-table-page.component.html",
  styleUrls: ["./licenses-table-page.component.less"],
  changeDetection: ChangeDetectionStrategy.OnPush,
  providers: [
    LicensesDataSource
  ],
  host: {
    class: "Licenses Page"
  }
})
@AuthorizePermissions(AppPermissions.LicenseKeysManage)
export class LicensesTablePageComponent extends DisposableComponentBase implements OnInit {
  typeRoutes = [
    {
      key: null,
      value: "All"
    },
    {
      key: "lifetime",
      value: "Lifetime"
    },
    {
      key: "renewal",
      value: "Renewal"
    }
  ] as KeyValuePair[];
  selectedType$ = new BehaviorSubject<string | null>(null);
  memberDetails$ = new BehaviorSubject<MemberSummaryData | null>(null);

  isCreateDialogVisible = false;
  plans$ = new BehaviorSubject<PlanData[]>([]);
  releases$ = new BehaviorSubject<ActiveReleaseInfoData[]>([]);

  activeFilters$ = new BehaviorSubject<ActiveFilter[]>([]);
  licensesSortKinds = {
    sort: "sort",
    planId: "planId",
    releaseId: "releaseId"
  };
  supportedFilters: ActiveFilter[] = [
    {
      displayName: "newest",
      kind: this.licensesSortKinds.sort,
      label: "Sort by new",
      value: LicensesSortBy.Newest,
      command: () => this.pushFilterToRoute(this.licensesSortKinds.sort, LicensesSortBy.Newest)
    },
    {
      displayName: "oldest",
      kind: this.licensesSortKinds.sort,
      label: "Sort by oldest",
      value: LicensesSortBy.Oldest,
      command: () => this.pushFilterToRoute(this.licensesSortKinds.sort, LicensesSortBy.Oldest)
    },
    {
      displayName: "release",
      kind: this.licensesSortKinds.releaseId,
      label: "Sort by release",
      value: null,
      command: () => this.isReleasePickerVisible = true
    },
    {
      displayName: "expiry",
      kind: this.licensesSortKinds.sort,
      label: "Sort by expiry",
      value: LicensesSortBy.Expiry,
      command: () => this.pushFilterToRoute(this.licensesSortKinds.sort, LicensesSortBy.Expiry)
    },
    {
      displayName: "plan",
      kind: this.licensesSortKinds.planId,
      label: "Sort by plan",
      value: null,
      command: () => this.isPlanPickerVisible = true
    },
  ];

  isPlanPickerVisible = false;
  isReleasePickerVisible = false;

  constructor(readonly licensesDataSource: LicensesDataSource,
              private router: Router,
              private activatedRoute: ActivatedRoute,
              private memberService: MemberRoleBindingsService,
              private plansService: PlansService,
              private releasesService: ReleasesService,
              private toolbarService: ToolbarService,
              readonly globalShadowService: GlobalShadowService) {
    super();
    toolbarService.setTitle("License Keys");
  }

  async ngOnInit(): Promise<void> {
    this.activatedRoute.queryParams
      .pipe(
        this.untilDestroy(),
      )
      .subscribe(async p => {
        const type = p.type;
        const sort: LicensesSortBy = p.sort;
        const planId = NumsUtil.parseOrNull(p.planId);
        const releaseId = NumsUtil.parseOrNull(p.releaseId);
        if (type && supportedTypes.indexOf(type) === -1) {
          await this.router.navigate([], {
            queryParams: null,
            relativeTo: this.activatedRoute,
            replaceUrl: true
          });
          return;
        }

        const activeFilters: ActiveFilter[] = [];
        if (sort) {
          activeFilters.push(this.supportedFilters.find(_ => _.kind === this.licensesSortKinds.sort && _.value === sort));
        }

        if (planId) {
          activeFilters.push({
            ...this.supportedFilters.find(_ => _.kind === this.licensesSortKinds.planId),
            value: planId
          });
        }

        if (releaseId) {
          activeFilters.push({
            ...this.supportedFilters.find(_ => _.kind === this.licensesSortKinds.releaseId),
            value: releaseId
          });
        }

        this.activeFilters$.next(activeFilters);

        const lifetimeOnly = !type ? undefined : type === "lifetime";
        this.licensesDataSource.setLifetimeOnly(lifetimeOnly);
        this.licensesDataSource.setPlanId(planId);
        this.licensesDataSource.setReleaseId(releaseId);
        this.licensesDataSource.setSortBy(sort);
        this.selectedType$.next(type);
      });

    const plans = await this.asyncTracker.executeAsAsync(
      this.plansService.plansGetAll().pipe(map(_ => _.payload)));

    this.plans$.next(plans);

    const releases = await this.asyncTracker.executeAsAsync(
      this.releasesService.releasesGetActiveList().pipe(map(_ => _.payload))
    );

    this.releases$.next(releases);
  }

  formatDate(rawDate: string): string {
    if (!rawDate) {
      return null;
    }

    const end = rawDate.indexOf("T");
    return rawDate.substring(0, end);
  }

  trackById = (_: number, r: LicenseKeySnapshotData) => r?.id;

  async showMemberDetails(licenseKeyData: LicenseKeySnapshotData): Promise<void> {
    this.memberDetails$.next(null);
    const member = await this.asyncTracker.executeAsAsync(
      this.memberService.memberRoleBindingsGetSummary(licenseKeyData.user.id)
    );

    this.memberDetails$.next(member.payload);
  }

  refresh(): Promise<any> {
    this.isCreateDialogVisible = false;
    return this.licensesDataSource.refreshData();
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
