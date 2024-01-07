import "echarts/lib/chart/line";
import "echarts/lib/chart/bar";
import "echarts/lib/component/tooltip";
import "echarts/lib/component/markLine";
import {ChangeDetectionStrategy, Component, OnInit} from "@angular/core";
import {BehaviorSubject} from "rxjs";
import * as moment from "moment";
import {Moment} from "moment";
import {ActivatedRoute, Router} from "@angular/router";
import {DisposableComponentBase} from "../../../shared/components/disposable.component-base";
import {
  ActivationDiscordInsightsData,
  AnalyticsService,
  DiscordInsightsAnalyticsApiContract,
  DiscordInsightsInterval,
  JoinBySourceDiscordInsightsData,
  LeaversDiscordInsightsData,
  MembershipDiscordInsightsData,
  OverviewDiscordInsightsData,
  RetentionDiscordInsightsData
} from "../../../dashboards-api";
import {AppPermissions} from "../../../core/models/permissions.model";
import {ToolbarService} from "../../../core/services/toolbar.service";
import {AuthorizePermissions} from "../../../core/decorators/permission.decorator";


const memberJoinageOptions = {
  color: ["#1C48FE", "#FF6628", "#FF6287"],
  grid: {
    left: 40,
    top: 50,
    right: 0,
    bottom: 30
  },
  xAxis: [
    {
      type: "category",
      data: [] as string[]
    }
  ],
  yAxis: [
    {
      type: "value"
    }
  ],
  series: [
    {
      name: "Invite",
      type: "bar",
      stack: "1",
      barMaxWidth: 31,
      barMinHeight: 1,
      data: [] as number[]
    },
    {
      name: "Vanity URL",
      type: "bar",
      stack: "1",
      barMaxWidth: 31,
      barMinHeight: 1,
      data: [] as number[]
    },
    {
      name: "Server Discovery",
      type: "bar",
      stack: "1",
      barMaxWidth: 31,
      barMinHeight: 1,
      data: [] as number[],
      itemStyle: {
        borderRadius: [100, 100, 0, 0]
      }
    }
  ]
};

const memberJoinageLineOptions = {
  title: {
    show: false,
  },
  color: ["#1C48FE"],
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
    top: 20,
    right: 0,
    bottom: 30
  },
  xAxis: {
    type: "category",
    data: [] as string[]
  },
  yAxis: {
    type: "value"
  },
  series: [{
    data: [] as number[],
    type: "line",
    smooth: true,
    // showSymbol: true,
    lineStyle: {
      width: 3
    }
  }]
};
const serverLeavesOverTimeOptions = {
  color: ["#FF6628"],
  barGap: "100%",
  grid: {
    left: 40,
    top: 50,
    right: 0,
    bottom: 30
  },
  xAxis: [
    {
      type: "category",
      data: [1, 2, 3, 4, 5, 7, 8, 9, 10, 11, 12]
    }
  ],
  yAxis: [
    {
      type: "value"
    }
  ],
  series: [
    {
      name: "1",
      type: "bar",
      data: [120, 132, 101, 134, 90, 230, 210, 101, 134, 90, 230, 210],
      barMaxWidth: 8,
      itemStyle: {
        borderRadius: [100, 100, 0, 0]
      }
    },
    {
      name: "2",
      type: "bar",
      data: [220, 182, 191, 234, 290, 330, 182, 191, 234, 290, 330, 310],
      barMaxWidth: 8,
      itemStyle: {
        borderRadius: [100, 100, 0, 0]
      }
    },
    {
      name: "3",
      type: "bar",
      data: [150, 232, 201, 154, 190, 330, 410, 201, 154, 190, 330, 410],
      barMaxWidth: 8,
      itemStyle: {
        borderRadius: [100, 100, 0, 0]
      }
    }
  ]
};

const successfulActivatesFirstDayOptions = {
  title: {
    show: false,
  },
  color: ["#E8E8E8", "#1C48FE", "#FF6628"],
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
  xAxis: [{
    // type: "category",
    data: [] as string[]
  }],
  yAxis: [
    {
      type: "value"
    }, {
      type: "value",
      max: 100,
      min: 0,
      show: false,
      axisLabel: {
        show: false
      }
    }
  ],
  series: [
    {
      name: "1",
      type: "bar",
      data: [] as number[],
      barMaxWidth: 8,
      z: -1,

      itemStyle: {
        borderRadius: [100, 100, 0, 0]
      }
    },
    {
      data: [] as number[],
      type: "line",
      yAxisIndex: 1,
      smooth: true,
      // showSymbol: true,
      markLine: {
        silent: true,
        data: [
          [
            {
              lineStyle: {
                color: "#000",
                type: "solid",
                width: 2
              },
              label: {
                // distance: [-400, -200],
                show: true,
                position: "insideStartTop",
                formatter: "Benchmark 30% Talked",
                fontSize: 10
              },
              symbol: "none",
              x: 40,
              yAxis: 30,
            },
            {
              symbol: "none",
              x: "100%",
              yAxis: 30,
            }
          ]
        ]
      },
      lineStyle: {
        width: 3
      }
    },
    {
      data: [] as number[],
      type: "line",
      yAxisIndex: 1,
      // showSymbol: false,
      smooth: true,
      lineStyle: {
        width: 3
      }
    }
  ]
};
const retainNextWeekOptions = {
  title: {
    show: false,
  },
  color: ["#E8E8E8", "#1C48FE"],
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
  xAxis: [{
    // type: "category",
    data: [] as string[]
  }],
  yAxis: [
    {
      type: "value"
    },
    {
      type: "value",
      max: 100,
      min: 0,
      show: false,
      axisLabel: {
        show: false
      }
    }
  ],
  series: [
    {
      name: "1",
      type: "bar",
      data: [] as number[],
      barMaxWidth: 8,
      z: -1,
      itemStyle: {
        borderRadius: [100, 100, 0, 0]
      }
    },
    {
      data: [] as number[],
      type: "line",
      yAxisIndex: 1,
      smooth: true,
      markLine: {
        silent: true,
        data: [
          [
            {
              lineStyle: {
                color: "#FF6628",
                type: "solid",
                width: 2
              },
              label: {
                // distance: [-400, -200],
                show: true,
                position: "insideStartTop",
                formatter: "Benchmark 20% Retained",
                fontSize: 10,
                color: "#FF6628"
              },
              symbol: "none",
              yAxis: 20,
              // xAxis: 0
              x: 40,
              // y: 280
            },
            {
              symbol: "none",
              x: "100%",
              // y: 280
              yAxis: 20,
              // xAxis: 0
            }
          ]
        ]
      },
      // showSymbol: true,
      lineStyle: {
        width: 3
      }
    }
  ]
};


const dates = {
  startDate: (d?: string | Date | Moment) => moment(d).startOf("day").toISOString(),
  endDate: (d?: string | Date | Moment) => moment(d).endOf("day").toISOString(),
};

const defaults = {
  startAt: () => dates.startDate(moment().add(-1, "month")),
  endAt: () => dates.endDate()
};

interface StatsTab {
  route: string[];
  title: string;
  permission: string;
}


interface NumStatsEntry {
  readonly percents: number;
  readonly curr: number;
  readonly prev: number;
}

class NumStatsEntryImpl implements NumStatsEntry {
  readonly percents: number = 0;
  readonly curr: number = 0;
  readonly prev: number = 0;

  constructor(curr: number, prev: number) {
    this.percents = NumStatsEntryImpl.calculatePercents(curr, prev);
    this.curr = +curr.toFixed(1);
    this.prev = +prev.toFixed(1);
  }

  static calculatePercents(curr: number, prev: number): number {
    const max = Math.max(curr, prev);
    const min = Math.min(curr, prev);
    let percents = +(!min && !max ? 0 : (max - min) / max * 100).toFixed(1);
    if (curr < prev) {
      percents = percents * -1;
    }

    return percents;
  }
}

@Component({
  selector: "app-analytics-discord",
  templateUrl: "./analytics-discord.component.html",
  styleUrls: ["./analytics-discord.component.less"],
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: {
    class: "Page"
  }
})
@AuthorizePermissions(AppPermissions.AnalyticsDiscordRead)
export class AnalyticsDiscordComponent extends DisposableComponentBase implements OnInit {
  memberJoinageOptions$ = new BehaviorSubject<any>({...memberJoinageOptions});
  memberJoinageLegend = [
    {label: "Invite", color: "#1C48FE"},
    {label: "Vanity URL", color: "#FF6628"},
    {label: "Server Discovery", color: "#FF6287"},
  ];
  memberJoinageLineOptions$ = new BehaviorSubject<any>({...memberJoinageLineOptions});
  memberJoinageLineLegend = [
    {label: "Members", color: "#1C48FE"},
  ];
  serverLeavesOverTimeOptions$ = new BehaviorSubject<any>({...serverLeavesOverTimeOptions});
  serverLeavesOverTimeLegend = [
    {label: "Members", color: "#FF6628"},
  ];
  successfulActivatesFirstDayOptions$ = new BehaviorSubject<any>({...successfulActivatesFirstDayOptions});
  successfulActivatesFirstDayLegend = [
    {label: "New Members", color: "#E8E8E8"},
    {label: "talked (voice or text)", color: "#1C48FE"},
    {label: "visit more than 3 channels", color: "#FF6628"},
  ];
  retainNextWeekOptions$ = new BehaviorSubject<any>({...retainNextWeekOptions});
  retainNextWeekLegend = [
    {label: "New Members", color: "#E8E8E8"},
    {label: "Week 1 retention", color: "#1C48FE"},
  ];

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
  period$ = new BehaviorSubject<DiscordInsightsInterval>(DiscordInsightsInterval.Daily);
  startAt$ = new BehaviorSubject<Date>(moment().add(-1, "month").toDate());
  endAt$ = new BehaviorSubject<Date>(new Date());
  newMembers$ = new BehaviorSubject<NumStatsEntry>(new NumStatsEntryImpl(0, 0));
  newCommunicators$ = new BehaviorSubject<NumStatsEntry>(new NumStatsEntryImpl(0, 0));
  newMemberRetention$ = new BehaviorSubject<NumStatsEntry>(new NumStatsEntryImpl(0, 0));

  constructor(private activatedRoute: ActivatedRoute,
              private router: Router,
              private toolbarService: ToolbarService,
              private analyticsService: AnalyticsService) {
    super();
    toolbarService.setTitle("Discord Analytics");
  }

  getFooterCardLabel(prefix: string): string | null {
    if (this.period$.value === DiscordInsightsInterval.Daily) {
      return null;
    }

    if (this.period$.value === DiscordInsightsInterval.Weekly) {
      return prefix + " last week";
    }

    return prefix + " last month";
  }

  ngOnInit(): void {
    this.activatedRoute.queryParams
      .pipe(this.untilDestroy())
      .subscribe(async p => {
        const startAt = p.startAt && moment(p.startAt);
        const endAt = p.endAt && moment(p.endAt);
        const period: DiscordInsightsInterval = p.period;

        if (
          !startAt || !startAt.isValid()
          || !endAt || !endAt.isValid() || startAt && endAt && endAt.isSameOrBefore(startAt)
          || !(period in DiscordInsightsInterval)
        ) {
          await this.router.navigate([], {
            queryParams: {
              startAt: defaults.startAt(),
              endAt: defaults.endAt(),
              period: DiscordInsightsInterval.Daily
            },
            replaceUrl: true,
            relativeTo: this.activatedRoute
          });
          return;
        }

        this.startAt$.next(startAt.toDate());
        this.endAt$.next(endAt.toDate());
        this.period$.next(period);

        const r = await this.asyncTracker.executeAsAsync(
          this.analyticsService.analyticsGetDiscord(period, startAt.toISOString(), endAt.toISOString())
        );

        const {
          overview: [currOverview, prevOverview],
          retention: [currRetention, prevRetention],
          joinBySource,
          membership,
          leavers,
          activation
        } = r.payload;

        const dateFormat = this.period$.value === DiscordInsightsInterval.Monthly ? "MMM YY" : "MMM DD";

        this.setNumericValues(currOverview, prevOverview, currRetention, prevRetention);
        this.setMembersJoinage(joinBySource.slice(), dateFormat);
        this.setMembership(membership.slice(), dateFormat);
        this.setLeavers(leavers.slice(), dateFormat);
        this.setActivations(activation.slice(), dateFormat);
        this.setRetention(r, dateFormat);
      });
  }

  private setRetention(r: DiscordInsightsAnalyticsApiContract, dateFormat: string) {
    const retentionData = {
      retention: [] as number[],
      members: [] as number[],
      xLabels: [] as string[]
    };

    for (const i of r.payload.retention.slice().reverse()) {
      retentionData.members.push(i.newMembers);
      retentionData.retention.push(+i.pctRetained.toFixed(1));
      retentionData.xLabels.push(moment(i.intervalStartTimestamp).format(dateFormat));
    }

    this.retainNextWeekOptions$.next({
      ...retainNextWeekOptions,
      xAxis: [
        {...retainNextWeekOptions.xAxis[0], data: retentionData.xLabels},
        {...retainNextWeekOptions.xAxis[1]}
      ],
      series: [
        {
          ...retainNextWeekOptions.series[0],
          data: retentionData.members
        },
        {
          ...retainNextWeekOptions.series[1],
          data: retentionData.retention
        }
      ]
    });
  }

  private setActivations(activation: Array<ActivationDiscordInsightsData>, dateFormat: string) {
    const activationsData = {
      visits: [] as number[],
      voices: [] as number[],
      members: [] as number[],
      xLabels: [] as string[]
    };

    for (const i of activation.reverse()) {
      activationsData.members.push(i.newMembers);
      activationsData.visits.push(+i.pctOpenedChannels.toFixed(1));
      activationsData.voices.push(+i.pctCommunicated.toFixed(1));
      activationsData.xLabels.push(moment(i.intervalStartTimestamp).format(dateFormat));
    }

    this.successfulActivatesFirstDayOptions$.next({
      ...successfulActivatesFirstDayOptions,
      xAxis: [
        {...successfulActivatesFirstDayOptions.xAxis[0], data: activationsData.xLabels}
      ],
      series: [
        {
          ...successfulActivatesFirstDayOptions.series[0],
          data: activationsData.members
        },
        {
          ...successfulActivatesFirstDayOptions.series[1],
          data: activationsData.visits
        },
        {
          ...successfulActivatesFirstDayOptions.series[2],
          data: activationsData.voices
        }
      ]
    });
  }

  private setNumericValues(currOverview: OverviewDiscordInsightsData, prevOverview: OverviewDiscordInsightsData,
                           currRetention: RetentionDiscordInsightsData, prevRetention: RetentionDiscordInsightsData) {
    const newMembers = new NumStatsEntryImpl(currOverview?.newMembers || 0, prevOverview?.newMembers || 0);
    const newCommunicators = new NumStatsEntryImpl(currOverview?.newCommunicators || 0, prevOverview?.newCommunicators || 0);
    const newMemberRetention = new NumStatsEntryImpl(currRetention?.pctRetained || 0, prevRetention?.pctRetained || 0);
    this.newMembers$.next(newMembers);
    this.newCommunicators$.next(newCommunicators);
    this.newMemberRetention$.next(newMemberRetention);
  }

  private setMembership(membership: Array<MembershipDiscordInsightsData>, dateFormat: string) {
    const membershipData = {
      data: new Array<number>(),
      labels: new Array<string>()
    };
    for (const i of membership.reverse()) {
      membershipData.labels.push(moment(i.intervalStartTimestamp).format(dateFormat));
      membershipData.data.push(i.totalMembership);
    }

    this.memberJoinageLineOptions$.next({
      ...memberJoinageLineOptions,
      xAxis: [
        {
          type: "category",
          data: membershipData.labels
        }
      ],
      series: [
        {
          ...memberJoinageLineOptions.series[0],
          data: membershipData.data
        }
      ]
    });
  }

  private setMembersJoinage(joinBySource: Array<JoinBySourceDiscordInsightsData>, dateFormat: string) {
    const joinBySourceData = {
      discovery: new Array<number>(),
      invites: new Array<number>(),
      vanity: new Array<number>(),
      labels: new Array<string>()
    };

    for (const i of joinBySource.reverse()) {
      joinBySourceData.discovery.push(i.discoveryJoins);
      joinBySourceData.invites.push(i.invites);
      joinBySourceData.vanity.push(i.vanityJoins);
      joinBySourceData.labels.push(moment(i.intervalStartTimestamp).format(dateFormat));
    }

    this.memberJoinageOptions$.next({
      ...memberJoinageOptions,
      xAxis: [
        {
          type: "category",
          data: joinBySourceData.labels
        }
      ],
      series: [
        {
          ...memberJoinageOptions.series[0],
          data: joinBySourceData.invites
        },
        {
          ...memberJoinageOptions.series[1],
          data: joinBySourceData.vanity
        },
        {
          ...memberJoinageOptions.series[2],
          data: joinBySourceData.discovery
        }
      ]
    });
  }

  private setLeavers(leavers: Array<LeaversDiscordInsightsData>, dateFormat: string) {
    const leaversData = {
      data: {} as any,
      labels: [] as string[]
    };

    for (const i of leavers.reverse()) {
      if (!(i.daysInGuild in leaversData.data)) {
        leaversData.data[i.daysInGuild] = [];
      }

      leaversData.data[i.daysInGuild].push(i.leavers);
      const label = moment(i.intervalStartTimestamp).format(dateFormat);
      if (leaversData.labels.indexOf(label) === -1) {
        leaversData.labels.push(label);
      }
    }

    const series = [] as any[];
    for (const k in leaversData.data) {
      if (!leaversData.data.hasOwnProperty(k)) {
        continue;
      }

      series.push({
        ...serverLeavesOverTimeOptions.series[0],
        data: leaversData.data[k],
        name: k
      });
    }

    this.serverLeavesOverTimeOptions$.next({
      ...serverLeavesOverTimeOptions,
      xAxis: [
        {
          type: "category",
          data: leaversData.labels
        }
      ],
      series
    });
  }

  async pushStartAt(value: Date) {
    await this.pushQuery({
      startAt: dates.startDate(value),
      endAt: dates.endDate(moment(value).add(1, "month"))
    });
  }

  async pushEndAt(value: Date) {
    const startAt = moment(this.startAt$.value).startOf("day");
    const endAt = moment(value);
    await this.pushQuery({
      endAt: dates.endDate(endAt),
      startAt: endAt.isSameOrBefore(startAt)
        ? dates.startDate(endAt.add(-1, "month"))
        : startAt.toISOString()
    });
  }

  async pushPeriod(value: DiscordInsightsInterval) {
    await this.pushQuery({
      // startAt: defaults.startAt(),
      // endAt: defaults.endAt(),
      period: value
    });
  }

  private async pushQuery(query: any) {
    await this.router.navigate([], {
      queryParams: query,
      relativeTo: this.activatedRoute,
      queryParamsHandling: "merge"
    });
  }
}
