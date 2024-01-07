import {FormControl, FormGroup} from "@angular/forms";

export const emptyBase64FileResource: Base64FileResource = {length: 0};

export interface Base64FileResource {
  name?: string;
  content?: string;
  contentType?: string;
  length: number;
}

export class Base64FileFormGroup extends FormGroup {
  constructor() {
    super({
      name: new FormControl(),
      content: new FormControl(),
      contentType: new FormControl(),
      length: new FormControl(),
    });
  }

  get nameCtrl(): FormControl {
    return this.get("name") as FormControl;
  }

  get contentCtrl(): FormControl {
    return this.get("content") as FormControl;
  }

  get contentTypeCtrl(): FormControl {
    return this.get("contentType") as FormControl;
  }

  get lengthCtrl(): FormControl {
    return this.get("length") as FormControl;
  }
}
