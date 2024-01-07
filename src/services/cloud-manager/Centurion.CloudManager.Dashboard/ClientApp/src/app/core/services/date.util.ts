import * as moment from "moment";

export class DateUtil {
  static fromNow(rawDate: string) {
      const date = moment(rawDate);
      if (date.isBefore(moment().startOf("day"))) {
        return date.format("llll");
      }

      if (date.isBefore(moment().add(-1, "hour"))) {
        return date.format("LT");
      }

      return date.fromNow();
  }
}
