import {AbstractControl, ValidatorFn} from "@angular/forms";

function isStrongPwd2(password: string): boolean {
  if (!password) {
    return false;
  }

  const uppercase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

  const lowercase = "abcdefghijklmnopqrstuvwxyz";

  const digits = "0123456789";

  const ucaseFlag = contains(password, uppercase);

  const lcaseFlag = contains(password, lowercase);

  const digitsFlag = contains(password, digits);


  return ucaseFlag && lcaseFlag && digitsFlag;
}

function contains(password: string, allowedChars: string): boolean {

  for (let i = 0; i < password.length; i++) {
    const char = password.charAt(i);

    if (allowedChars.indexOf(char) >= 0) {
      return true;
    }

  }

  return false;
}

export class AppValidators {

  static passwordNotWeek(sourceControl: AbstractControl) {
    const value = sourceControl.value;

    return isStrongPwd2(value) ? null : {passwordNotWeek: true};
  }
  static notEmpty(sourceControl: AbstractControl) {
    const value: any[] = sourceControl.value;

    return !!value.length ? null : {notEmpty: true};
  }

  static compare(compareControlName: string): ValidatorFn {
    return (sourceControl: AbstractControl) => {
      const form = sourceControl.parent;
      if (!form) {
        return null;
      }

      const compareControl = form.get(compareControlName);

      if (!compareControl) {
        return null;
      }

      const sourceValue = sourceControl.value;
      const confirm = compareControl.value;

      if (!confirm || confirm.length <= 0) {
        return null;
      }

      if (confirm !== sourceValue) {
        return {
          doesNotMatch: true
        };
      }

      return null;
    };
  }

  static gt(compareControlProvider: () => AbstractControl): ValidatorFn {
    return (sourceControl: AbstractControl) => {
      const form = sourceControl.parent;
      if (!form) {
        return null;
      }

      const compareControl = compareControlProvider();

      if (!compareControl) {
        return null;
      }

      const sourceValue = Number(sourceControl.value);
      const shouldBeGreater = Number(compareControl.value);
      //
      // if (shouldBeGreater.length <= 0) {
      //   return null;
      // }

      if (sourceValue <= shouldBeGreater) {
        return {
          gt: {value: shouldBeGreater}
        };
      }

      return null;
    };
  }
}
