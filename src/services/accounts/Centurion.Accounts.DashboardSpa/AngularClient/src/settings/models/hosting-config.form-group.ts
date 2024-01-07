import {FormControl, FormGroup, Validators} from "@angular/forms";

export class HostingConfigFormGroup extends FormGroup {
  constructor() {
    super({
      domainName: new FormControl(null, [Validators.required]),
      mode: new FormControl(null, [Validators.required]),
    });
  }

  get domainNameCtrl(): FormControl {
    return this.get("domainName") as FormControl;
  }

  get modeCtrl(): FormControl {
    return this.get("mode") as FormControl;
  }
}
