import {AbstractControl, FormControl, FormGroup, Validators} from "@angular/forms";
import {SaveReleaseCommand} from "../../dashboards-api";

export class ReleaseFormGroup extends FormGroup {
  constructor(data?: SaveReleaseCommand) {
    super({
      id: new FormControl(data?.id),
      title: new FormControl(data?.title, [Validators.required]),
      initialStock: new FormControl(data?.initialStock || 0, [Validators.required]),
      type: new FormControl(data?.type || 0, [Validators.required]),
      planId: new FormControl(data?.planId, [Validators.required]),
      password: new FormControl(data?.password || Math.round(Date.now() * Math.random()).toString(16), [Validators.required]),
      isActive: new FormControl(!!data?.isActive)
    });
  }

  get titleCtrl(): AbstractControl {
    return this.get("title")
  }

  get initialStockCtrl(): AbstractControl {
    return this.get("initialStock")
  }

  get typeCtrl(): AbstractControl {
    return this.get("type")
  }

  get planIdCtrl(): AbstractControl {
    return this.get("planId")
  }

  get passwordCtrl(): AbstractControl {
    return this.get("password")
  }

  get isActiveCtrl(): AbstractControl {
    return this.get("isActive")
  }
}
