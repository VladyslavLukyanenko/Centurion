import {getLogger as getLoggerImpl, setDefaultLevel as setDefaultLevelImpl, levels as levelsImpl, default as logImpl} from "loglevel";
import {LoglevelPluginPrefixOptions, reg, apply} from "loglevel-plugin-prefix";

reg(logImpl);

export interface Logger {
  trace(msg: string): void;
  debug(msg: string): void;
  info(msg: string): void;
  warn(msg: string): void;
  error(msg: string): void;
  setLevel(level: any): void;
}

export interface SupportedLoggerLevels {
  trace: any;
  debug: any;
  info: any;
  warn: any;
  error: any;
}

const formattingDefaults: LoglevelPluginPrefixOptions = {
  template: "[%t %l] %n:",
  timestampFormatter: function (date) {
    return date.toISOString();
  },
};

export const levels: SupportedLoggerLevels = {
  trace: levelsImpl.TRACE,
  debug: levelsImpl.DEBUG,
  info: levelsImpl.INFO,
  warn: levelsImpl.WARN,
  error: levelsImpl.ERROR,
};

export const setDefaultLevel = (level: any) => setDefaultLevelImpl(level);
export const getLogger = (name: string): Logger => {
  const logger = getLoggerImpl(name);
  apply(logger, formattingDefaults);

  return logger;
};
