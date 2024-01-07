import {ChangeDetectionStrategy, Component, EventEmitter, Input, Output} from "@angular/core";
import {PlansService} from "../../../dashboards-api";
import {DisposableComponentBase} from "../../../shared/components/disposable.component-base";
import {FormUtil} from "../../../core/services/form.util";
import {PlanFormGroup} from "../../models/plan.form-group";
import {KeyValuePair} from "../../../core/models/key-value-pair.model";

@Component({
  selector: "app-plan-edit-dialog",
  templateUrl: "./plan-edit-dialog.component.html",
  styleUrls: ["./plan-edit-dialog.component.less"],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class PlanEditDialogComponent extends DisposableComponentBase {
  form = new PlanFormGroup();
  @Input() isVisible = false;
  @Output() isVisibleChange = new EventEmitter<boolean>();
  @Output() saved = new EventEmitter<void>();

  supportedCurrencies: KeyValuePair[] = [
    { key: "USD", value: "USD" },
    { key: "EUR", value: "EUR" },
    { key: "UAH", value: "UAH" }
  ];
  constructor(private plansService: PlansService) {
    super();
  }

  async create(): Promise<void> {
    if (this.form.invalid) {
      FormUtil.validateAllFormFields(this.form);
      return;
    }

    await this.asyncTracker.executeAsAsync(
      this.plansService.plansCreate(this.form.value)
    );

    this.saved.emit();
  }
}
