// This file can be replaced during build by using the `fileReplacements` array.
// `ng build ---prod` replaces `environment.ts` with `environment.prod.ts`.
// The list of file replacements can be found in `angular.json`.

import {levels} from "loglevel";

export const environment = {
  defaultCultureCode: "en-US",
  production: false,
  apiHostName: "http://localhost:5000",
  authApiHostName: "http://localhost:5000",
  logLevel: levels.DEBUG,
  publicProjectName: "Monitors Panel",
  fileSizeLimitBytes: 100_485_760, // 100Mb
  supportedImageTypes: ".gif,.png,.jpg,.bmp,.tiff,.tif",

  auth: {
    clientSecret: "SECRET:monitors-panel-api",
    clientId: "monitors-panel-api",
    loginGrantType: "password",
    refreshGrantType: "refresh_token",


    redirectAuthenticatedRoute: "admin",
    loginRoute: "account",
    refreshTokenLifetime: 3600,
    adminRoleName: "admin"
  },

  unixBeginEpochDate: "1970/01/01"
};

/*
 * In development mode, to ignore zone related error stack frames such as
 * `zone.run`, `zoneDelegate.invokeTask` for easier debugging, you can
 * import the following file, but please comment it out in production mode
 * because it will have performance impact when throw error
 */
// import 'zone.js/dist/zone-error';  // Included with Angular CLI.
