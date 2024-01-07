import {Injectable} from "@angular/core";
import {Observable} from "rxjs";
import {getLogger, Logger} from "./logging.service";

@Injectable({
  providedIn: "root"
})
export class SchedulerService {
  private timers: { [key: string]: any } = Object.create(null);

  private logger: Logger;
  constructor() {
    this.logger = getLogger("SchedulerService");
  }

  scheduleAsyncJob(jobId: string, observableFactory: () => Observable<any>, intervalFactory: () => number, onError: (error: any) => void) {
    this.cancelScheduledJob(jobId);
    const interval = intervalFactory();
    this.logger.debug(`Scheduled async job '${jobId}' in ${interval}ms`);
    this.timers[jobId] = setTimeout(() => {
      observableFactory().subscribe(() => {
        this.scheduleAsyncJob(jobId, observableFactory, intervalFactory, onError);
      }, onError);
    }, interval);
  }

  scheduleJob(jobId: string, action: () => void, intervalFactory: () => number) {
    this.cancelScheduledJob(jobId);
    const interval = intervalFactory();
    this.timers[jobId] = setTimeout(() => {
      action();
      this.scheduleJob(jobId, action, intervalFactory);
    }, interval);
  }

  cancelScheduledJob(jobId: string) {
    if (jobId in this.timers) {
      const timer = this.timers[jobId];
      clearTimeout(timer);
      this.logger.debug(`Canceled scheduled job '${jobId}'`);
    }
  }
}
