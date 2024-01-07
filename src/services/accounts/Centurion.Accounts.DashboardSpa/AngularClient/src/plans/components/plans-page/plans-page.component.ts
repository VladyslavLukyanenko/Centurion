import {ChangeDetectionStrategy, Component, OnInit} from "@angular/core";
import {BehaviorSubject, combineLatest} from "rxjs";
import {PlanData, PlansService} from "../../../dashboards-api";
import {ActivatedRoute, Router} from "@angular/router";
import {ConfirmationService} from "primeng/api";
import {map} from "rxjs/operators";
import {DisposableComponentBase} from "../../../shared/components/disposable.component-base";
import {Observable} from "rxjs/internal/Observable";
import {ToolbarService} from "../../../core/services/toolbar.service";
import {AppPermissions} from "../../../core/models/permissions.model";
import {AuthorizePermissions} from "../../../core/decorators/permission.decorator";

@Component({
  selector: "app-plans-page",
  templateUrl: "./plans-page.component.html",
  styleUrls: ["./plans-page.component.less"],
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: {
    class: "Page Releases"
  }
})
@AuthorizePermissions(AppPermissions.PlansManage)
export class PlansPageComponent extends DisposableComponentBase implements OnInit {
  plans$ = new BehaviorSubject<PlanData[]>([]);

  noData$: Observable<boolean>;
  isCreateDialogVisible = false;

  appPermissions = AppPermissions;

  constructor(private plansService: PlansService,
              private router: Router,
              private toolbarService: ToolbarService,
              private activatedRoute: ActivatedRoute,
              private confirmationService: ConfirmationService) {
    super();
    toolbarService.setTitle("Plans");
  }

  async ngOnInit(): Promise<void> {
    this.noData$ = combineLatest([this.asyncTracker.isLoading$, this.plans$])
      .pipe(map(([isl, p]) => !isl && !p.length));

    this.activatedRoute.queryParams
      .pipe(
        this.untilDestroy(),
      )
      .subscribe(async p => {
      });
    await this.refresh();
  }

  trackById = (_: number, r: PlanData) => r?.id;

  remove(r: PlanData, e: Event): void {
    this.confirmationService.confirm({
      target: e.target,
      message: `Are you sure to remove "${r.description}" plan?`,
      icon: "pi pi-exclamation-triangle",
      accept: async () => {
        await this.asyncTracker.executeAsAsync(this.plansService.plansRemove(r.id));
        await this.refresh();
      }
    });
  }

  async refresh(): Promise<void> {
    this.isCreateDialogVisible = false;
    const plans = await this.asyncTracker.executeAsAsync(
      this.plansService.plansGetAll().pipe(map(_ => _.payload)));

    this.plans$.next(plans);
  }
}
