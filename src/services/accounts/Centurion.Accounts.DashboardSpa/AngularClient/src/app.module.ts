import {BrowserModule} from "@angular/platform-browser";
import {NgModule} from "@angular/core";
import {FormsModule, ReactiveFormsModule} from "@angular/forms";
import {HttpClientModule} from "@angular/common/http";
import {BrowserAnimationsModule} from "@angular/platform-browser/animations";

import {AppRoutingModule} from "./app-routing.module";

import {CoreModule} from "./core/core.module";


import {AppComponent} from "./app.component";
import {JWT_OPTIONS, JwtModule} from "@auth0/angular-jwt";
import {SharedModule} from "./shared/shared.module";
import {TokenService} from "./core/services/token.service";
import {environment} from "./environments/environment";
import {BASE_PATH as MONOLITHIC_API_BASE_PATH} from "./dashboards-api";
import {NgxEchartsModule} from "ngx-echarts";
import {ResiliencyInterceptorProvider} from "./core/services/interceptors/resiliency.interceptor";
import {ActivityInterceptorProvider} from "./core/services/interceptors/activity.interceptor";
import {QuillModule} from "ngx-quill";


@NgModule({
  declarations: [
    AppComponent,
  ],
  imports: [
    BrowserModule.withServerTransition({appId: "ng-cli-universal"}),

    NgxEchartsModule.forRoot({
      echarts: () => import("echarts")
    }),
    BrowserAnimationsModule,
    HttpClientModule,
    ReactiveFormsModule,
    FormsModule,
    AppRoutingModule,

    CoreModule,
    SharedModule,
    JwtModule.forRoot({
      jwtOptionsProvider: {
        provide: JWT_OPTIONS,
        useFactory: jwtOptionsFactory,
        deps: [TokenService]
      }
    }),

    QuillModule.forRoot({
      modules: {
        toolbar: [
          ["bold", "italic", { header: 1}, /*"image",*/ "link", { list: "ordered"}, { list: "bullet" }, "blockquote", "code-block"]
        ]
      }
    }),
  ],

  providers: [
    {provide: MONOLITHIC_API_BASE_PATH, useValue: environment.apiHostUrl},
    ActivityInterceptorProvider,
    ResiliencyInterceptorProvider,
  ],
  bootstrap: [AppComponent]
})
export class AppModule {
}

export function jwtOptionsFactory(auth: TokenService): any {
  return {
    tokenGetter: () => auth.encodedAccessToken,
    skipWhenExpired: true,
    whitelistedDomains: [
      /.*/
    ]
  };
}

