import {FormControl, FormGroup} from "@angular/forms";
import {ProductFeatureData} from "../../dashboards-api";
import {Base64FileFormGroup} from "../../shared/models/base64-file.resource";

export class ProductFeatureFormGroup extends FormGroup {
  constructor(data?: ProductFeatureData) {
    super({
      icon: new FormControl(data?.icon),
      uploadedIcon: new Base64FileFormGroup(),
      title: new FormControl(data?.title),
      desc: new FormControl(data?.desc),
    });
  }


  get iconCtrl(): FormControl {
    return this.get("icon") as FormControl;
  }

  get uploadedIconCtrl(): Base64FileFormGroup {
    return this.get("uploadedIcon") as Base64FileFormGroup;
  }

  get titleCtrl(): FormControl {
    return this.get("title") as FormControl;
  }

  get descCtrl(): FormControl {
    return this.get("desc") as FormControl;
  }

}
