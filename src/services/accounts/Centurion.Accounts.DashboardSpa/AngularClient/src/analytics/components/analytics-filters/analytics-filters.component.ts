import {ChangeDetectionStrategy, Component, EventEmitter, Input, Output} from '@angular/core';
import {GeneralPeriodTypes} from "../../../core/models/period-types.model";
import {KeyValuePair} from "../../../core/models/key-value-pair.model";
import * as moment from "moment";

const format = "yyyy-MM";

const maxYear = (new Date()).getFullYear();
const minYear = 1970;
const years = [...new Array(maxYear - minYear + 1)]
  .map((_, idx) => ({
    key: minYear + idx,
    value: minYear + idx
  }))
  .reverse();

const dateFormats = {
  [GeneralPeriodTypes.Yearly]: "yy",
  [GeneralPeriodTypes.Monthly]: "yy-mm"
}

@Component({
  selector: 'app-analytics-filters',
  templateUrl: './analytics-filters.component.html',
  styleUrls: ['./analytics-filters.component.less'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AnalyticsFiltersComponent {
  @Input() period!: GeneralPeriodTypes;
  @Input() startAt!: number | string;

  @Output() periodChange = new EventEmitter<GeneralPeriodTypes>();
  @Output() startAtChange = new EventEmitter<number | string>();

  years = years;
  periods = [
    {value: GeneralPeriodTypes.Monthly, key: "Monthly"},
    {value: GeneralPeriodTypes.Yearly, key: "Yearly"},
  ] as KeyValuePair<string, GeneralPeriodTypes>[];


  get dateFormat(): string {
    return dateFormats[this.period];
  }

  get isMonthlySelected() {
    return this.period === GeneralPeriodTypes.Monthly;
  }

  dispatchPeriodChange(p: GeneralPeriodTypes) {
    this.periodChange.emit(p);
  }

  dispatchStartAtChange(p: string | Date) {
    if (p instanceof Date) {
      this.startAtChange.emit(moment(p).format(format));
      return;
    }

    this.startAtChange.emit(p);
  }
}
