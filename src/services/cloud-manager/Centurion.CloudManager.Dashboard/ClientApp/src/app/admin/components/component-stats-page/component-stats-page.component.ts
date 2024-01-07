import {ChangeDetectionStrategy, Component, NgZone, OnInit} from "@angular/core";
import {BehaviorSubject} from "rxjs";
import {DisposableComponentBase} from "../../../shared/components/disposable.component-base";
import {ComponentStatsEntry, ComponentStatsService} from "../../../monitors-panel-api";

interface GroupedStats {
  type: string;
  data: ComponentStatsEntry[]
}

@Component({
  selector: "r-component-stats-page",
  templateUrl: "./component-stats-page.component.html",
  styleUrls: ["./component-stats-page.component.scss"],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ComponentStatsPageComponent extends DisposableComponentBase implements OnInit {

  stats$ = new BehaviorSubject<GroupedStats[]>([]);

  constructor(private statsService: ComponentStatsService) {
    super();
  }

  async ngOnInit() {
    await this.fetchStats();
  }

  private async fetchStats(): Promise<void> {
    const r = await this.asyncTracker.executeAsAsync(this.statsService.componentStatsGetStats());
    const keys = Object.keys(r.payload);
    const data = keys.map(k => ({
      type: k,
      data: r.payload[k].sort((a, b) => a.componentName > b.componentName ? 1 : -1)
    }))
      .sort((a, b) => a.type > b.type ? 1 : -1);

    this.stats$.next(data);
  }
}
