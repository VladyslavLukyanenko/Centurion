import {FormControl, FormGroup} from "@angular/forms";
import {PlanData} from "../../dashboards-api";

export class PlanFormGroup extends FormGroup {
  constructor(data?: PlanData) {
    super({
      id: new FormControl(data?.id),
      description: new FormControl(data?.description),
      amount: new FormControl(data?.amount),
      currency: new FormControl(data?.currency),
      subscriptionPlan: new FormControl(data?.subscriptionPlan),
      licenseLifeDays: new FormControl(data?.licenseLifeDays),
      unbindableDelayDays: new FormControl(data?.unbindableDelayDays),
      isTrial: new FormControl(data?.isTrial),
      trialPeriodDays: new FormControl(data?.trialPeriodDays),
      discordRoleId: new FormControl(data?.discordRoleId),
      protectPurchasesWithCaptcha: new FormControl(data?.protectPurchasesWithCaptcha),
    });
  }


  get idCtrl(): FormControl {
    return this.get("id") as FormControl;
  }

  get descriptionCtrl(): FormControl {
    return this.get("description") as FormControl;
  }

  get amountCtrl(): FormControl {
    return this.get("amount") as FormControl;
  }

  get currencyCtrl(): FormControl {
    return this.get("currency") as FormControl;
  }

  get subscriptionPlanCtrl(): FormControl {
    return this.get("subscriptionPlan") as FormControl;
  }

  get licenseLifeDaysCtrl(): FormControl {
    return this.get("licenseLifeDays") as FormControl;
  }

  get unbindableDelayDaysCtrl(): FormControl {
    return this.get("unbindableDelayDays") as FormControl;
  }

  get isTrialCtrl(): FormControl {
    return this.get("isTrial") as FormControl;
  }

  get trialPeriodDaysCtrl(): FormControl {
    return this.get("trialPeriodDays") as FormControl;
  }

  get discordRoleIdCtrl(): FormControl {
    return this.get("discordRoleId") as FormControl;
  }

  get protectPurchasesWithCaptchaCtrl(): FormControl {
    return this.get("protectPurchasesWithCaptcha") as FormControl;
  }
}
