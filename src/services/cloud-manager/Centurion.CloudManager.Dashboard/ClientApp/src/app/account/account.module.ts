import {NgModule} from "@angular/core";
import {SharedModule} from "../shared/shared.module";
import {LoginPageComponent} from "./components/login-page/login-page.component";
import {LoginFormComponent} from "./components/login-form/login-form.component";
import {AccountRoutingModule} from "./account-routing.module";
import {TranslateLoader, TranslateModule} from "@ngx-translate/core";
import {httpLoaderFactory} from "../ngx-translate-http-loader";
import {HttpClient} from "@angular/common/http";


@NgModule({
  declarations: [LoginPageComponent, LoginFormComponent],
  imports: [
    SharedModule,
    AccountRoutingModule,

    TranslateModule.forChild({
      loader: {
        provide: TranslateLoader,
        useFactory: httpLoaderFactory,
        deps: [HttpClient]
      }
    })
  ]
})
export class AccountModule {
}
