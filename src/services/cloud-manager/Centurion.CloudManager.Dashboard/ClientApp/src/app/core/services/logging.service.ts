import {getLogger as getLoggerImpl, setDefaultLevel as setDefaultLevelImpl, levels as levelsImpl, default as logImpl} from "loglevel";
import prefix, {LoglevelPluginPrefixOptions} from "loglevel-plugin-prefix";

prefix.reg(logImpl);

export interface Logger {
  trace(msg: string);

  debug(msg: string);

  info(msg: string);

  warn(msg: string);

  error(msg: string);

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
  prefix.apply(logger, formattingDefaults);

  return logger;
};
