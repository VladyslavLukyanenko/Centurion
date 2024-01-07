import {levels} from "loglevel";

export const environment = {
  defaultCultureCode: "en-US",
  production: true,
  apiHostName: "https://api.monitor.builders",
  authApiHostName: "https://api.monitor.builders",
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
