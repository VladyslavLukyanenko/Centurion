import * as moment from "moment";
import {DurationInputArg2, Moment} from "moment";

export class DateUtil {
  static startOfDay(d?: string): string {
    return moment(d).startOf("day").toISOString();
  }

  static endOfDay(d?: string): string {
    return moment(d).endOf("day").toISOString();
  }

  static toISOString(d?: string | Moment): string {
    return moment(d).toISOString();
  }

  static fromNow(rawDate: string): string {
    const date = moment(rawDate);
    if (date.isBefore(moment().startOf("day"))) {
      return date.format("llll");
    }

    if (date.isBefore(moment().add(-1, "hour"))) {
      return date.format("LT");
    }

    return date.fromNow();
  }

  static valueOrFuture(date: any, amount: number, unit: DurationInputArg2): string {
    if (!date) {
      date = moment().add(amount, unit);
    }

    return DateUtil.startOfDay(date);
  }

  static humanizeDate(rawDate: string | Moment): string {
    const date = moment(rawDate);
    if (date.isBefore(moment().startOf("day"))
      || date.isAfter(moment().endOf("day"))) {
      return date.format("LL");
    }

    return date.fromNow();
  }
}
