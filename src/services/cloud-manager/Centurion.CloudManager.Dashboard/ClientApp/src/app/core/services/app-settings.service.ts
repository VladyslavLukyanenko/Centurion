import {BehaviorSubject, Observable} from "rxjs";
import {Injectable, OnDestroy, Optional, SkipSelf} from "@angular/core";

import * as moment from "moment";
import {distinctUntilChanged, distinctUntilKeyChanged, filter, map} from "rxjs/operators";
import {environment} from "../../../environments/environment";
import {TranslateService} from "@ngx-translate/core";
import {en_US, NzI18nService} from "ng-zorro-antd";
import {getLogger} from "./logging.service";

const nzI18nCfg = {
  "en-US": en_US,
  "fallback": en_US
};

export interface Culture {
  isoCode: string;
  friendlyName: string;
}

const preferredCultureKey = "monitors-panel.preferredCulture";

export function defaultLocaleProvider(): string {
  return localStorage.getItem(preferredCultureKey) || environment.defaultCultureCode;
}

@Injectable({
  providedIn: "root"
})
export class AppSettingsService {
  private _log = getLogger("AppSettingsService");

  constructor(
    @Optional()
    @SkipSelf()
      alreadyLoadedSettings: AppSettingsService,
    private translateService: TranslateService,
    private nzLocalizationService: NzI18nService
  ) {
    if (alreadyLoadedSettings) {
      throw new Error("This service MUST be singleton!");
    }

    this.nzLocalizationService.setLocale(nzI18nCfg[environment.defaultCultureCode] || nzI18nCfg.fallback);
    this.translateService.use(environment.defaultCultureCode).toPromise();
  }
}
