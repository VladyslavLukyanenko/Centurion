import {ChangeDetectionStrategy, Component, OnInit} from "@angular/core";
import {GeneralPeriodTypes} from "../../../core/models/period-types.model";
import {ActivatedRoute, Router} from "@angular/router";
import {DisposableComponentBase} from "../../../shared/components/disposable.component-base";
import {BehaviorSubject} from "rxjs";
import {NumsUtil} from "../../../core/services/nums.util";
import {AppPermissions} from "../../../core/models/permissions.model";

interface StatsTab {
  route: string[];
  title: string;
  permission: string;
}

const yearMonthPattern = /\d{4}-\d{2}/;
const isStartAtValid = (period: GeneralPeriodTypes, startAt: string) => {
  return period === GeneralPeriodTypes.Yearly && !isNaN(+startAt)
    || period === GeneralPeriodTypes.Monthly && yearMonthPattern.test(startAt);
};

const now = new Date();
const startAtDefaults = {
  [GeneralPeriodTypes.Monthly]: `${now.getFullYear()}-${NumsUtil.toLeftPaddedStr(now.getMonth() + 1)}`,
  [GeneralPeriodTypes.Yearly]: now.getFullYear()
};

@Component({
  selector: "app-analytics-dashboard-page",
  templateUrl: "./analytics-dashboard-page.component.html",
  styleUrls: ["./analytics-dashboard-page.component.less"],
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: {
    class: "Page"
  }
})
export class AnalyticsDashboardPageComponent extends DisposableComponentBase implements OnInit {
  tabs = [
    {
      route: ["general"],
      title: "General",
      permission: AppPermissions.AnalyticsGeneralRead
    }, {
      route: ["discord"],
      title: "Discord",
      permission: AppPermissions.AnalyticsDiscordRead
    },
  ] as StatsTab[];
  period$ = new BehaviorSubject<GeneralPeriodTypes>(GeneralPeriodTypes.Yearly);
  startAt$ = new BehaviorSubject<number | string>(startAtDefaults[this.period$.value]);

  constructor(readonly activatedRoute: ActivatedRoute,
              private router: Router) {
    super();
  }

  ngOnInit(): void {
    this.activatedRoute.queryParams
      .pipe(
        this.untilDestroy()
      )
      .subscribe(async p => {
        const period = p.period;
        const startAt = period === GeneralPeriodTypes.Yearly ? +p.startAt : p.startAt;
        if (!period || !(period in GeneralPeriodTypes) || !startAt || !isStartAtValid(period, startAt)) {
          await this.router.navigate([], {
            relativeTo: this.activatedRoute,
            queryParams: {period: GeneralPeriodTypes.Yearly, startAt: (new Date()).getFullYear()},
            replaceUrl: true
          });
          return;
        }

        this.startAt$.next(startAt);
        this.period$.next(period);
      });
  }

  pushStartAt(startAt: string | number): Promise<any> {
    return this.router.navigate([], {
      queryParams: {
        startAt
      },
      queryParamsHandling: "merge",
      relativeTo: this.activatedRoute
    });
  }

  pushPeriod(period: GeneralPeriodTypes): Promise<any> {
    return this.router.navigate([], {
      queryParams: {
        period,
        startAt: startAtDefaults[period]
      },
      queryParamsHandling: "merge",
      relativeTo: this.activatedRoute
    });
  }

}
