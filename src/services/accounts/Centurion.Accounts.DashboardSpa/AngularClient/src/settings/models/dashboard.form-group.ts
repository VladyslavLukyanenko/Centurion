import {FormControl, FormGroup, Validators} from "@angular/forms";
import {Base64FileFormGroup} from "../../shared/models/base64-file.resource";
import {StripeIntegrationConfigFormGroup} from "./stripe-integration-config.form-group";
import {DiscordConfigFormGroup} from "./discord-config.form-group";
import {HostingConfigFormGroup} from "./hosting-config.form-group";
import {ProductInfoFormGroup} from "./product-info.form-group";

export class DashboardFormGroup extends FormGroup {
  constructor() {
    super({
      id: new FormControl(),
      stripeConfig: new StripeIntegrationConfigFormGroup(),
      // expiresAt: new FormControl(),
      productInfo: new ProductInfoFormGroup(),
      discordConfig: new DiscordConfigFormGroup(),
      timeZoneId: new FormControl(null, [Validators.required]),
      hostingConfig: new HostingConfigFormGroup(),
      chargeBackersExportEnabled: new FormControl(),
    });
  }

  get idCtrl(): FormControl {
    return this.get("id") as FormControl;
  }

  get stripeConfigCtrl(): StripeIntegrationConfigFormGroup {
    return this.get("stripeConfig") as StripeIntegrationConfigFormGroup;
  }

  get expiresAtCtrl(): FormControl {
    return this.get("expiresAt") as FormControl;
  }

  get discordConfigCtrl(): DiscordConfigFormGroup {
    return this.get("discordConfig") as DiscordConfigFormGroup;
  }

  get productInfoCtrl(): ProductInfoFormGroup {
    return this.get("productInfo") as ProductInfoFormGroup;
  }

  get timeZoneIdCtrl(): FormControl {
    return this.get("timeZoneId") as FormControl;
  }

  get hostingConfigCtrl(): HostingConfigFormGroup {
    return this.get("hostingConfig") as HostingConfigFormGroup;
  }

  get chargeBackersExportEnabledCtrl(): FormControl {
    return this.get("chargeBackersExportEnabled") as FormControl;
  }
}
