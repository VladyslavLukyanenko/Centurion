import {ChangeDetectionStrategy, Component, Input} from '@angular/core';

@Component({
  selector: 'app-chart-legend',
  templateUrl: './chart-legend.component.html',
  styleUrls: ['./chart-legend.component.less'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ChartLegendComponent {
  @Input() items!: any[];
  @Input() itemLabelKey!: string;
  @Input() itemColorKey!: string;
}
