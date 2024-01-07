import {ChangeDetectionStrategy, Component, Input} from '@angular/core';

@Component({
  selector: 'app-stats-entry',
  templateUrl: './stats-entry.component.html',
  styleUrls: ['./stats-entry.component.less'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class StatsEntryComponent {
  @Input() value!: any;
  @Input() percents!: number;
}
