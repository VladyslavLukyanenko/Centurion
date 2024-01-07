import {Injectable, Optional, SkipSelf} from "@angular/core";
import {getLogger} from "./logging.service";
import * as moment from "moment";

@Injectable({
  providedIn: "root"
})
export class AppSettingsService {
  private _log = getLogger("AppSettingsService");

  constructor(
    @Optional()
    @SkipSelf()
      alreadyLoadedSettings: AppSettingsService,
  ) {
    if (alreadyLoadedSettings) {
      throw new Error("This service MUST be singleton!");
    }

    moment.locale("en-US");
  }
}
