import {levels} from "loglevel";
export const environment = {
  production: true,
  logLevel: levels.WARN,
  publicProjectName: "Centurion Dashboard",
  apiHostUrl: "https://accounts-api.centurion.gg",
  fileSizeLimitBytes: 10485760,

  // todo: take this config from API. because its client specific
  stripe: {
    publicKey: ""
  },


  auth: {
    discord: {
      codeKey: "gg.centurion.dashboard.auth.discord.code"
    }
  }
};
