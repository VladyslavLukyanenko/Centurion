import {FormControl, FormGroup, Validators} from "@angular/forms";
import {GenerateLicenseKeysCommand} from "../../dashboards-api";

export class LicensesGenerateFormGroup extends FormGroup {
  constructor(data?: GenerateLicenseKeysCommand) {
    super({
      isLifetime: new FormControl(!!data?.isLifetime, [Validators.required]),
      planId: new FormControl(data?.planId, [Validators.required]),
      trialDaysCount: new FormControl(data?.trialDaysCount || 30, [Validators.required]),
      quantity: new FormControl(data?.quantity || 50, [Validators.required]),
      allowUnbinding: new FormControl(data?.allowUnbinding == null ? true : data?.allowUnbinding),
    });
  }

  get isLifetimeCtrl() {
    return this.get("isLifetime");
  }

  get planIdCtrl() {
    return this.get("planId");
  }

  get trialDaysCountCtrl() {
    return this.get("trialDaysCount");
  }

  get quantityCtrl() {
    return this.get("quantity");
  }

  get allowUnbindingCtrl() {
    return this.get("allowUnbinding");
  }
}
