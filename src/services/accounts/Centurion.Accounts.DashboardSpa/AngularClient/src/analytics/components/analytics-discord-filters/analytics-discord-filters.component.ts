import {ChangeDetectionStrategy, Component, EventEmitter, Input, OnInit, Output} from '@angular/core';
import {KeyValuePair} from "../../../core/models/key-value-pair.model";
import {DiscordInsightsInterval} from "../../../dashboards-api";

const dateFormats = {
  [DiscordInsightsInterval.Monthly]: "yy-mm",
  [DiscordInsightsInterval.Weekly]: "yy-mm-dd",
  [DiscordInsightsInterval.Daily]: "yy-mm-dd",
}
@Component({
  selector: 'app-analytics-discord-filters',
  templateUrl: './analytics-discord-filters.component.html',
  styleUrls: ['./analytics-discord-filters.component.less'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AnalyticsDiscordFiltersComponent implements OnInit {
  @Input() period!: DiscordInsightsInterval;
  @Input() startAt: Date;
  @Input() endAt: Date;

  @Output() startAtChange = new EventEmitter<Date>();
  @Output() endAtChange = new EventEmitter<Date>();
  @Output() periodChange = new EventEmitter<DiscordInsightsInterval>();

  periods = [
    {value: DiscordInsightsInterval.Monthly, key: DiscordInsightsInterval.Monthly},
    {value: DiscordInsightsInterval.Weekly, key: DiscordInsightsInterval.Weekly},
    {value: DiscordInsightsInterval.Daily, key: DiscordInsightsInterval.Daily},
  ] as KeyValuePair<string, DiscordInsightsInterval>[];
  constructor() { }

  ngOnInit(): void {
  }


  get dateFormat(): string {
    return dateFormats[this.period];
  }

  get isMonthlySelected() {
    return this.period === DiscordInsightsInterval.Monthly;
  }

  dispatchStartAtChange(p: Date) {
    this.startAtChange.emit(p);
  }

  dispatchEndAtChange(p: Date) {
    this.endAtChange.emit(p);
  }
}
