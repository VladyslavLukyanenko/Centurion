import {ChangeDetectionStrategy, Component} from "@angular/core";

@Component({
  selector: "app-not-found",
  templateUrl: "./not-found.component.html",
  styleUrls: ["./not-found.component.less"],
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: {
    class: "Page"
  }
})
export class NotFoundComponent {
}
