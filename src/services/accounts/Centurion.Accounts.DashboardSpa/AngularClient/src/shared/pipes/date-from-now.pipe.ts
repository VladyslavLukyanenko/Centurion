import { Pipe, PipeTransform } from "@angular/core";
import {DateUtil} from "../../core/services/date.util";

@Pipe({
  name: "dateFromNow"
})
export class DateFromNowPipe implements PipeTransform {

  transform(value: any, ...args: any[]): any {
    return DateUtil.fromNow(value);
  }

}
