import {ChangeDetectionStrategy, Component, EventEmitter, Output} from "@angular/core";
import {ControlContainer} from "@angular/forms";
import {ProductFeatureFormGroup} from "../../models/product-feature.form-group";

@Component({
  selector: "app-product-feature-editor",
  templateUrl: "./product-feature-editor.component.html",
  styleUrls: ["./product-feature-editor.component.less"],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ProductFeatureEditorComponent {
  @Output() removeClick = new EventEmitter<void>();

  constructor(private container: ControlContainer) {
  }

  get form(): ProductFeatureFormGroup {
    return this.container.control as ProductFeatureFormGroup;
  }
}
