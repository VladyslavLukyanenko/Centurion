import {RouterModule} from "@angular/router";
import {NgModule, Optional, SkipSelf} from "@angular/core";
import {HttpClient} from "@angular/common/http";
import {TranslateLoader, TranslateModule} from "@ngx-translate/core";


import {httpLoaderFactory} from "../ngx-translate-http-loader";
import {throwIfAlreadyLoaded} from "./module-import-guard";
import {SharedModule} from "../shared/shared.module";
import {LayoutComponent} from "./components/layout/layout.component";
import {Configuration} from "../monitors-panel-api";
import {environment} from "../../environments/environment";

@NgModule({
  imports: [
    RouterModule,

    SharedModule,

    TranslateModule.forChild({
      loader: {
        provide: TranslateLoader,
        useFactory: httpLoaderFactory,
        deps: [HttpClient]
      }
    }),
  ],
  declarations: [
    LayoutComponent,
  ],
  providers: [

    {
      provide: Configuration,
      useFactory: () => new Configuration({
        basePath: environment.apiHostName,
        credentials: ({
          Bearer: () => {
            return null;
            // const token = tokenService.encodedAccessToken!;
            // return token && "Bearer " + token || null;
          }
        })
      }),
      // deps: [TokenService]
    }

  ],
  exports: [
  ],
  entryComponents: []
})
export class CoreModule {
  constructor(
    @Optional()
    @SkipSelf()
      parentModule: CoreModule
  ) {
    throwIfAlreadyLoaded(parentModule, "CoreModule");
  }
}
