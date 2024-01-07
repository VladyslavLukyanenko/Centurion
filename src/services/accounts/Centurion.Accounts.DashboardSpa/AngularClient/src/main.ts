import {enableProdMode} from "@angular/core";
import {platformBrowserDynamic} from "@angular/platform-browser-dynamic";

import {environment} from "./environments/environment";
import {getLogger, setDefaultLevel} from "./core/services/logging.service";
import {AppModule} from "./app.module";

setDefaultLevel(environment.logLevel);
const log = getLogger("[main]");

if (environment.production) {
  enableProdMode();
}

platformBrowserDynamic().bootstrapModule(AppModule)
  .catch((err: any) => console.error(err));
