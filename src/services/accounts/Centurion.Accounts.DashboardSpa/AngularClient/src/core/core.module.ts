import {RouterModule} from "@angular/router";
import {NgModule, Optional, SkipSelf} from "@angular/core";

import {LayoutComponent} from "./components/layout/layout.component";

import {throwIfAlreadyLoaded} from "./module-import-guard";
import {SharedModule} from "../shared/shared.module";

import {MainMenuComponent} from "./components/main-menu/main-menu.component";
import {ToolbarComponent} from "./components/toolbar/toolbar.component";
import {ProfileWidgetComponent} from "./components/profile-widget/profile-widget.component";
import {BrowserAnimationsModule} from "@angular/platform-browser/animations";
import {SidebarComponent} from './components/sidebar/sidebar.component';
import {LogoComponent} from './components/logo/logo.component';
import {NotFoundComponent} from './components/not-found/not-found.component';
import {ApiModule, Configuration} from "../dashboards-api";
import {TokenService} from "./services/token.service";
import {environment} from "../environments/environment";
import {
  GLOBAL_SEARCH_RESULT_MANAGER,
  LicenseKeySearchClickHandler, ReleasesKeySearchClickHandler
} from "./services/global-search/global-search-result.manager";
import { ForbiddenComponent } from './components/forbidden/forbidden.component';

@NgModule({
  imports: [
    RouterModule,
    BrowserAnimationsModule,

    SharedModule,
    ApiModule
  ],
  declarations: [
    LayoutComponent,

    MainMenuComponent,
    ToolbarComponent,
    ProfileWidgetComponent,
    SidebarComponent,
    LogoComponent,
    NotFoundComponent,
    ForbiddenComponent,
  ],
  exports: [LayoutComponent],
  providers: [
    {
      provide: Configuration,
      useFactory: (tokenService: TokenService) => new Configuration({
        basePath: environment.apiHostUrl,
        credentials: ({
          Bearer: () => {
            const token = tokenService.encodedAccessToken;
            return token && "Bearer " + token || null;
          }
        })
      }),
      deps: [TokenService]
    },
    {provide: GLOBAL_SEARCH_RESULT_MANAGER, multi: true, useClass: LicenseKeySearchClickHandler},
    {provide: GLOBAL_SEARCH_RESULT_MANAGER, multi: true, useClass: ReleasesKeySearchClickHandler},
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
