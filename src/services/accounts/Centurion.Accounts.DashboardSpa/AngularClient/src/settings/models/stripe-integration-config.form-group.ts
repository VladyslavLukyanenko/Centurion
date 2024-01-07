import {FormControl, FormGroup} from "@angular/forms";

export class StripeIntegrationConfigFormGroup extends FormGroup {
  constructor() {
    super({
      apiKey: new FormControl(),
      webHookEndpointSecret: new FormControl(),
    });
  }

  get apiKeyCtrl(): FormControl {
    return this.get("apiKey") as FormControl;
  }

  get webHookEndpointSecretCtrl(): FormControl {
    return this.get("webHookEndpointSecret") as FormControl;
  }
}
