import {ChangeDetectionStrategy, Component, EventEmitter, Input, Output} from "@angular/core";
import {DisposableComponentBase} from "../disposable.component-base";

@Component({
  selector: "app-confirm-dialog",
  templateUrl: "./confirm-dialog.component.html",
  styleUrls: ["./confirm-dialog.component.less"],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ConfirmDialogComponent extends DisposableComponentBase {
  @Input() isVisible = false;
  @Input() message: string;
  @Input() title: string;
  @Input() confirmText = "Yes";
  @Input() cancelText = "Cancel";
  @Input() type: "primary" | "danger" = "primary";

  @Output() confirmed = new EventEmitter<void>();
  @Output() cancelled = new EventEmitter<void>();
  @Output() isVisibleChange = new EventEmitter<boolean>();
  constructor() {
    super();
  }
}
