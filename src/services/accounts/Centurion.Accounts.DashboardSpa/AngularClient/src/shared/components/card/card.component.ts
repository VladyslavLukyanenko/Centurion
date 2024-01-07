import {ChangeDetectionStrategy, Component, Input} from "@angular/core";

@Component({
  selector: "app-card",
  templateUrl: "./card.component.html",
  styleUrls: ["./card.component.less"],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class CardComponent {
  @Input() title!: string;
  @Input() footer?: string;
}
