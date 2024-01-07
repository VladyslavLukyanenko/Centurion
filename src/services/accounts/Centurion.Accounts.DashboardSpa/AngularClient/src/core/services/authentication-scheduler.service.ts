import {Injectable} from "@angular/core";
import {AuthenticationService} from "./authentication.service";
import {Observable} from "rxjs";
import {TokenService} from "./token.service";
import {getLogger, Logger} from "./logging.service";

const MAX_TIMEOUT = 2147483647;
const TOKEN_RENEWAL_JOB_ID = "gg.centurion.dashboard.auth.scheduler.token.renewal";

@Injectable({
  providedIn: "root"
})
export class AuthenticationSchedulerService {
  private timers: { [key: string]: any } = Object.create(null);
  private logger: Logger;

  constructor(
    private auth: AuthenticationService,
    private token: TokenService,
  ) {
    this.logger = getLogger("AuthenticationSchedulerService");
  }

  scheduleTokenRenewal(): void {
    if (!this.auth.canReauthenticate()) {
      return;
    }

    this.scheduleAsyncJob(
      TOKEN_RENEWAL_JOB_ID,
      () => this.auth.reauthenticate(),
      this.intervalMillisFactory,
      err => {
        this.auth.logOut();
        return err;
      });
  }

  cancelTokenRenewal(): void {
    this.cancelScheduledJob(TOKEN_RENEWAL_JOB_ID);
  }

  private intervalMillisFactory = () => {
    const now = Date.now();
    const expirationDate = Math.min(this.token.accessTokenExpirationDate?.valueOf(), this.token.refreshTokenExpirationDate.valueOf());
    const tokenLifetimePeriod = expirationDate - now;
    const timeBefore = Math.min(30 * 1000, tokenLifetimePeriod / 2); // 30 sec
    return Math.min(tokenLifetimePeriod - timeBefore, MAX_TIMEOUT);
  }

  private scheduleAsyncJob(jobId: string, observableFactory: () => Observable<any>,
                           intervalFactory: () => number, onError: (error: any) => void): void {
    this.cancelScheduledJob(jobId);
    const interval = intervalFactory();
    this.logger.debug(`Scheduled async job '${jobId}' in ${interval}ms`);
    this.timers[jobId] = setTimeout(() => {
      observableFactory()
        .subscribe(() => {
          this.scheduleAsyncJob(jobId, observableFactory, intervalFactory, onError);
        }, onError);
    }, interval);
  }

  //
  // private scheduleJob(jobId: string, action: () => void, intervalMillisFactory: () => number) {
  //   this.cancelScheduledJob(jobId);
  //   const interval = intervalMillisFactory();
  //   this.timers[jobId] = setTimeout(() => {
  //     action();
  //     this.scheduleJob(jobId, action, intervalMillisFactory);
  //   }, interval);
  // }

  private cancelScheduledJob(jobId: string) {
    if (jobId in this.timers) {
      const timer = this.timers[jobId];
      clearTimeout(timer);
      this.logger.debug(`Canceled scheduled job '${jobId}'`);
    }
  }
}
