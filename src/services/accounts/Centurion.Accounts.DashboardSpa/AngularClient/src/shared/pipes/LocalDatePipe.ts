import {Pipe, PipeTransform} from "@angular/core";
import * as moment from "moment";
import {Moment} from "moment";

@Pipe({name: "localDate"})
export class LocalDatePipe implements PipeTransform {
  transform(date: Moment | Date | string, format?: string): string {
    if (!format) {
      format = "LL";
    }

    return moment(date).format(format);
  }
}
