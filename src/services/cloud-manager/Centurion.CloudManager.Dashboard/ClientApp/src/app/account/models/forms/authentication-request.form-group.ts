import {FormControl, FormGroup, Validators} from "@angular/forms";
import {AuthenticationRequest} from "../../../core/models/authentication-request.model";

export class AuthenticationRequestFormGroup extends FormGroup {
  constructor(data: AuthenticationRequest = {} as any) {
    super({
      username: new FormControl(data.username, [Validators.required, Validators.email]),
      password: new FormControl(data.password, [Validators.required])
    });
  }
}
