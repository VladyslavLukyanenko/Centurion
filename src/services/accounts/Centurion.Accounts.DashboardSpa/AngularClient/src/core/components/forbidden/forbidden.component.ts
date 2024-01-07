import {ChangeDetectionStrategy, Component} from "@angular/core";

@Component({
  selector: "app-forbidden",
  templateUrl: "./forbidden.component.html",
  styleUrls: ["./forbidden.component.less"],
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: {
    class: "Page"
  }
})
export class ForbiddenComponent {
}
