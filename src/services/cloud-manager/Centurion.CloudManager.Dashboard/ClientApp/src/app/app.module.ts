import {BrowserModule} from "@angular/platform-browser";
import {NgModule} from "@angular/core";
import {FormsModule, ReactiveFormsModule} from "@angular/forms";
import {HttpClient, HttpClientModule} from "@angular/common/http";

import {AppComponent} from "./app.component";
import {en_US, NZ_I18N, } from "ng-zorro-antd";
/** config angular i18n **/
import {registerLocaleData} from "@angular/common";
import en from "@angular/common/locales/en";
import {BrowserAnimationsModule} from "@angular/platform-browser/animations";
import {CoreModule} from "./core/core.module";
import {AppRoutingModule} from "./app-routing.module";
import {SharedModule} from "./shared/shared.module";
import {TranslateLoader, TranslateModule} from "@ngx-translate/core";
import {httpLoaderFactory} from "./ngx-translate-http-loader";
import {JWT_OPTIONS, JwtModule} from "@auth0/angular-jwt";
import {TokenService} from "./core/services/token.service";
import {HostComponent} from "./host.component";
import {ApiModule, BASE_PATH} from "./monitors-panel-api";
import { environment } from "../environments/environment";

registerLocaleData(en);

@NgModule({
  declarations: [
    AppComponent,
    HostComponent,
  ],
  imports: [
    BrowserModule.withServerTransition({appId: "ng-cli-universal"}),
    BrowserAnimationsModule,
    HttpClientModule,
    ReactiveFormsModule,
    FormsModule,
    AppRoutingModule,

    CoreModule,
    SharedModule,

    ApiModule,
    JwtModule.forRoot({
      jwtOptionsProvider: {
        provide: JWT_OPTIONS,
        useFactory: jwtOptionsFactory,
        deps: [TokenService]
      }
    }),

    TranslateModule.forRoot({
      loader: {
        provide: TranslateLoader,
        useFactory: httpLoaderFactory,
        deps: [HttpClient]
      }
    })
  ],
  /** config ng-zorro-antd i18n (language && date) **/
  providers   : [
    { provide: NZ_I18N, useValue: en_US },
    { provide: BASE_PATH, useValue: environment.apiHostName }
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }


export function jwtOptionsFactory(auth: TokenService) {
  return {
    tokenGetter: () => auth.encodedAccessToken,
    skipWhenExpired: true,
    whitelistedDomains: [
      /.*/
    ]
  };
}
