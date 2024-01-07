import {FormArray, FormGroup} from "@angular/forms";

export class FormUtil {

  static validateAllFormFields(formGroup: FormGroup | FormArray) {
    // This code also works in IE 11
    Object.keys(formGroup.controls).forEach(field => {
      const control = formGroup.get(field);

      if (control instanceof FormGroup) {
        this.validateAllFormFields(control);
      } else if (control instanceof FormArray) {
        this.validateAllFormFields(control);
      }

      control.markAsTouched({onlySelf: true});
      control.updateValueAndValidity();
    });

    formGroup.updateValueAndValidity();
  }
}
