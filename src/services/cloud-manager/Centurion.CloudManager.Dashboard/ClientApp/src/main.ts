import { enableProdMode } from '@angular/core';
import { platformBrowserDynamic } from '@angular/platform-browser-dynamic';
import {setDefaultLevel, getLogger} from "./app/core/services/logging.service";

import { AppModule } from './app/app.module';
import { environment } from './environments/environment';

setDefaultLevel(environment.logLevel);
const log = getLogger("[main]");


export function getBaseUrl() {
  return document.getElementsByTagName('base')[0].href;
}

const providers = [
  { provide: 'BASE_URL', useFactory: getBaseUrl, deps: [] }
];

if (environment.production) {
  enableProdMode();
}

console.log("%c MonitorsPanel", "font-weight:bold;font-size:50px;color:red;text-shadow:3px 3px 0 rgb(217,31,38),6px 6px 0 rgb(226,91,14),9px 9px 0 rgb(245,221,8),12px 12px 0 rgb(5,148,68),15px 15px 0 rgb(2,135,206),18px 18px 0 rgb(4,77,145),21px 21px 0 rgb(42,21,113)");
platformBrowserDynamic(providers).bootstrapModule(AppModule)
  .catch(err => log.error(err));
