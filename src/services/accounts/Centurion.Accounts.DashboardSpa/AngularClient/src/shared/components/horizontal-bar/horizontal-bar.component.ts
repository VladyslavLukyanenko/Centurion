import {ChangeDetectionStrategy, Component, Input} from '@angular/core';

@Component({
  selector: 'app-horizontal-bar',
  templateUrl: './horizontal-bar.component.html',
  styleUrls: ['./horizontal-bar.component.less'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class HorizontalBarComponent {
  @Input() items!: any[];
  @Input() itemLabelKey!: string;
  @Input() itemWidthKey!: string;
  @Input() itemColorKey!: string;
}
