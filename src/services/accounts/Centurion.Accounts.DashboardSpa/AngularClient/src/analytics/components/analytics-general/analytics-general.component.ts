import "echarts/lib/chart/line";
import "echarts/lib/chart/bar";
import "echarts/lib/component/tooltip";
import {ChangeDetectionStrategy, Component, OnInit} from "@angular/core";
import {GeneralPeriodTypes} from "../../../core/models/period-types.model";
import {AnalyticsService, DecimalValueDiff, Int32ValueDiff, SingleValueDiff} from "../../../dashboards-api";
import {ActivatedRoute, Router} from "@angular/router";
import {DisposableComponentBase} from "../../../shared/components/disposable.component-base";
import {BehaviorSubject} from "rxjs";
import {NumsUtil} from "../../../core/services/nums.util";
import {AppPermissions} from "../../../core/models/permissions.model";
import {ToolbarService} from "../../../core/services/toolbar.service";
import {AuthorizePermissions} from "../../../core/decorators/permission.decorator";


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

const incomeOptions = {
  title: {
    show: false,
  },
  color: ["#1C48FE", "#FF6628"],
  tooltip: {
    trigger: "item",
    backgroundColor: "#F3F2F2",
    position: "top",
    formatter: "{c}",
    // extraCssText: "backdrop-filter: blur(10px); border: none;",
    textStyle: {
      fontFamily: "Inter, sans-serif"
    }
  },
  grid: {
    left: 40,
    top: 60,
    right: 0,
    bottom: 30
  },
  xAxis: {
    type: "category",
    data: [] as number[], // [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12]
  },
  yAxis: {
    type: "value"
  },
  series: [{
    data: [] as number[], // [820, 932, 901, 934, 1290, 1330, 1320, 820, 932, 901, 1330, 1420],
    type: "line",
    smooth: true,
    // showSymbol: true,
    lineStyle: {
      width: 3
    }
  }, {
    data: [] as number[], // [820, 932, 901, 934, 1290, 1330, 1320, 820, 932, 901, 1330, 1420].reverse(),
    type: "line",
    // showSymbol: false,
    smooth: true,
    lineStyle: {
      width: 3
    }
  }]
};

const visitorsOptions = {
  color: ["#1C48FE"],
  title: {
    show: false,
  },
  grid: {
    left: -15,
    top: 0,
    right: -15,
    bottom: 0
  },
  xAxis: {
    show: false,
    type: "category",
    data: [] as number[], // [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12],
  },
  yAxis: {
    show: false,
    type: "value"
  },
  series: [{
    data: [] as number[], // [820, 932, 901, 934, 1290, 1330, 1320, 820, 932, 901, 1330, 1420],
    type: "line",
    areaStyle: {
      color: {
        type: "linear",
        x: 0,
        y: 0,
        x2: 0,
        y2: 1,
        colorStops: [{
          offset: 0.03,
          color: "#AFBFFF" // color at 0% position
        }, {
          offset: 1,
          color: "#D8E7FF" // color at 100% position
        }],
      }
    },
    smooth: true,
    showSymbol: false,
    lineStyle: {
      width: 2
    }
  }]
};

const padLeft = (placeholders: string, value: number) => {
  const str = String(value);
  return placeholders.substr(0, placeholders.length - str.length) + str;
};

const nullo = Object.create({
  current: 0,
  previous: 0,
  changePercents: 0
});

@Component({
  selector: "app-analytics-general",
  templateUrl: "./analytics-general.component.html",
  styleUrls: ["./analytics-general.component.less"],
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: {
    class: "Page"
  }
})
@AuthorizePermissions(AppPermissions.AnalyticsGeneralRead)
export class AnalyticsGeneralComponent extends DisposableComponentBase implements OnInit {
  incomeOptions$ = new BehaviorSubject<any>({...incomeOptions});
  visitorsOptions$ = new BehaviorSubject<any>({...visitorsOptions});
  period = GeneralPeriodTypes.Yearly;


  totalRevenue$ = new BehaviorSubject<DecimalValueDiff>(nullo);
  totalUsers$ = new BehaviorSubject<Int32ValueDiff>(nullo);
  keysSold$ = new BehaviorSubject<Int32ValueDiff>(nullo);
  retentionRate$ = new BehaviorSubject<SingleValueDiff>(nullo);
  liveViewsCount$ = new BehaviorSubject<Int32ValueDiff>(nullo);
  visitorsCount$ = new BehaviorSubject<Int32ValueDiff>(nullo);

  incomesLegend = [
    {label: "This Year", color: "#4C6FFF"},
    {label: "Last Year", color: "#FF6628"},
  ];
  avgLiveViews$ = new BehaviorSubject<any[]>([
    {color: "#0062FF", label: "Desktop", width: "60%"},
    {color: "#FF6628", label: "Mobile", width: "40%"},
  ]);

  tabs = [
    {
      route: ["../general"],
      title: "General",
      permission: AppPermissions.AnalyticsGeneralRead
    }, {
      route: ["../discord"],
      title: "Discord",
      permission: AppPermissions.AnalyticsDiscordRead
    },
  ] as StatsTab[];
  period$ = new BehaviorSubject<GeneralPeriodTypes>(GeneralPeriodTypes.Yearly);
  startAt$ = new BehaviorSubject<number | string>(startAtDefaults[this.period$.value]);

  constructor(private analyticsService: AnalyticsService,
              private activatedRoute: ActivatedRoute,
              private toolbarService: ToolbarService,
              private router: Router) {
    super();
    toolbarService.setTitle("General Analytics");
  }

  getFooterCardLabel(prefix: string): string | null {
    const suffix = this.period$.value === GeneralPeriodTypes.Monthly
      ? " last month"
      : " last year";

    return prefix + suffix;
  }

  ngOnInit(): void {
    this.activatedRoute.queryParams
      .pipe(
        this.untilDestroy()
      )
      .subscribe(async p => {
        const
          period = p.period,
          startAt = period === GeneralPeriodTypes.Yearly ? +p.startAt : p.startAt;


        this.startAt$.next(startAt);
        this.period$.next(period);
        this.totalRevenue$.next(nullo);
        this.totalUsers$.next(nullo);
        this.keysSold$.next(nullo);
        this.retentionRate$.next(nullo);
        this.liveViewsCount$.next(nullo);
        this.incomeOptions$.next({...incomeOptions});
        this.visitorsOptions$.next({...visitorsOptions});
        this.visitorsCount$.next(nullo);
        if (!period || !(period in GeneralPeriodTypes) || !startAt || !isStartAtValid(period, startAt)) {
          await this.router.navigate([], {
            relativeTo: this.activatedRoute,
            queryParams: {period: GeneralPeriodTypes.Yearly, startAt: (new Date()).getFullYear()},
            replaceUrl: true
          });
          return;
        }

        const pad = "00";
        const offsetHours = (new Date).getTimezoneOffset() / 60 * -1;
        const h = Math.abs(Math.floor(offsetHours));
        const m = Math.abs(Math.abs(offsetHours) - h);
        const offset = `${offsetHours > 0 ? "+" : "-"}${padLeft(pad, h)}:${padLeft(pad, m)}`;

        const r = await this.asyncTracker.executeAsAsync(
          this.analyticsService.analyticsGetGeneral(offset, startAt, period));
        const data = r.payload;

        this.totalRevenue$.next(data.totalRevenue);
        this.totalUsers$.next(data.totalUsers);
        this.keysSold$.next(data.keysSold);
        this.retentionRate$.next(data.retentionRate);
        this.liveViewsCount$.next(data.avgLiveViews.liveViewsCount);
        this.visitorsCount$.next(data.visitors.count);

        const avgLiveViews = data.avgLiveViews;
        const avgTotal = avgLiveViews.desktopsCount + avgLiveViews.mobileCount;
        this.avgLiveViews$.next(
          [
            {...this.avgLiveViews$.value[0], width: (avgLiveViews.desktopsCount / avgTotal * 100).toFixed(1) + "%"},
            {...this.avgLiveViews$.value[1], width: (avgLiveViews.mobileCount / avgTotal * 100).toFixed(1) + "%"}
          ]
        );

        this.incomeOptions$.next({
          ...incomeOptions,
          xAxis: {
            ...incomeOptions.xAxis,
            data: data.income.map(_ => _.groupUnit)
          },
          series: [
            {
              ...incomeOptions.series[0],
              data: data.income.map(_ => _.amount.current)
            },
            {
              ...incomeOptions.series[1],
              data: data.income.map(_ => _.amount.previous)
            }
          ]
        });

        this.visitorsOptions$.next({
          ...this.visitorsOptions$.value,
          xAxis: {
            ...visitorsOptions.xAxis,
            data: data.visitors.data.map((r: any) => r.groupUnit)
          },
          series: [
            {
              ...this.visitorsOptions$.value.series[0],
              data: data.visitors.data.map((r: any) => r.value)
            }
          ]
        });
      });
  }

  pushStartAt(startAt: string | number) {
    return this.router.navigate([], {
      queryParams: {
        startAt
      },
      queryParamsHandling: "merge",
      relativeTo: this.activatedRoute
    });
  }

  pushPeriod(period: GeneralPeriodTypes) {
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
