import {FormArray, FormControl, FormGroup, Validators} from "@angular/forms";
import {ProductInfoData} from "../../dashboards-api";
import {ProductFeatureFormGroup} from "./product-feature.form-group";
import {Base64FileFormGroup} from "../../shared/models/base64-file.resource";

export class ProductInfoFormGroup extends FormGroup {
  constructor(data?: ProductInfoData) {
    super({
      name: new FormControl(data?.name, [Validators.required]),
      description: new FormControl(data?.description, [Validators.required]),
      version: new FormControl(data?.version, [Validators.required]),

      imageSrc: new FormControl(data?.imageSrc),
      logoSrc: new FormControl(data?.logoSrc),
      features: new FormArray((data?.features || []).map(e => new ProductFeatureFormGroup(e))),

      uploadedImage: new Base64FileFormGroup(),
      uploadedLogo: new Base64FileFormGroup(),
    });
  }

  get nameCtrl(): FormControl {
    return this.get("name") as FormControl;
  }

  get descriptionCtrl(): FormControl {
    return this.get("description") as FormControl;
  }

  get versionCtrl(): FormControl {
    return this.get("version") as FormControl;
  }

  get logoSrcCtrl(): FormControl {
    return this.get("logoSrc") as FormControl;
  }

  get imageSrcCtrl(): FormControl {
    return this.get("imageSrc") as FormControl;
  }

  get featuresGroup(): FormArray {
    return this.get("features") as FormArray;
  }


  get features(): ProductFeatureFormGroup[] {
    return this.featuresGroup.controls as ProductFeatureFormGroup[];
  }

  get uploadedImageCtrl(): Base64FileFormGroup {
    return this.get("uploadedImage") as Base64FileFormGroup;
  }

  get uploadedLogoCtrl(): Base64FileFormGroup {
    return this.get("uploadedLogo") as Base64FileFormGroup;
  }

  addEmptyFeatureEditor(): void {
    this.featuresGroup.push(new ProductFeatureFormGroup());
  }

  removeFeatureAt(ix: number): void {
    this.featuresGroup.removeAt(ix);
  }

  patchValue(value: ProductInfoData, options?: { onlySelf?: boolean; emitEvent?: boolean }): void {
    super.patchValue(value, options);
    for (const f of value.features) {
      this.featuresGroup.push(new ProductFeatureFormGroup(f));
    }
  }

  reset(value?: any, options?: { onlySelf?: boolean; emitEvent?: boolean }): void {
    super.reset(value, options);
    this.featuresGroup.clear();
  }
}
