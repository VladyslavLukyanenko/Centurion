import {ChangeDetectionStrategy, Component, EventEmitter, Output} from "@angular/core";

@Component({
  selector: "app-sidebar",
  templateUrl: "./sidebar.component.html",
  styleUrls: ["./sidebar.component.less"],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class SidebarComponent {
  @Output()
  navigated = new EventEmitter<void>();
}
