import {ChangeDetectionStrategy, Component} from "@angular/core";
import {DisposableComponentBase} from "../../../shared/components/disposable.component-base";

@Component({
  selector: "app-layout",
  templateUrl: "./layout.component.html",
  styleUrls: ["./layout.component.scss"],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class LayoutComponent extends DisposableComponentBase {
  isSidebarOpened = false;
}

