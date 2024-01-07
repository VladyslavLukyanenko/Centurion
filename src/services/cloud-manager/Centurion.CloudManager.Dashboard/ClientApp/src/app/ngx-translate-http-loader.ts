// NOTICE: required for AOT compilation    SidenavService

import {HttpClient} from "@angular/common/http";
import {TranslateHttpLoader} from "@ngx-translate/http-loader";

let translateHttpLoader: TranslateHttpLoader;

export function httpLoaderFactory(http: HttpClient) {
  if (!translateHttpLoader) {
    translateHttpLoader = new TranslateHttpLoader(http);
  }

  return translateHttpLoader;
}
