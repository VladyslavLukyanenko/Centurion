import {ChangeDetectionStrategy, Component, OnInit} from "@angular/core";
import {AuthenticationService} from "../../../core/services/authentication.service";
import {FormUtil} from "../../../core/services/form.util";
import {DisposableComponentBase} from "../../../shared/components/disposable.component-base";
import {ActivatedRoute, Router} from "@angular/router";
import {RoutesProvider} from "../../../core/services/routes.provider";
import {AuthenticationRequestFormGroup} from "../../models/forms/authentication-request.form-group";

@Component({
  selector: "r-login-form",
  templateUrl: "./login-form.component.html",
  styleUrls: ["./login-form.component.scss"],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class LoginFormComponent extends DisposableComponentBase implements OnInit {

  form = new AuthenticationRequestFormGroup();

  constructor(private authService: AuthenticationService,
              private router: Router,
              private activatedRoute: ActivatedRoute,
              private routesProvider: RoutesProvider) {
    super();
  }

  ngOnInit() {
    this.isLoading$
      .pipe(this.untilDestroy())
      .subscribe(isLoading => isLoading ? this.form.disable() : this.form.enable());
  }

  async authenticate() {
    if (this.form.invalid) {
      FormUtil.validateAllFormFields(this.form);
      return;
    }

    const success = await this.executeAsAsync(this.authService.authenticate(this.form.value));
    if (!success) {
      this.form.setErrors({
        invalidCredentials: true
      });
      return;
    }

    await this.executeAsync(this.router.navigate(this.routesProvider.getAuthenticatedRedirectUrl()));
  }
}
