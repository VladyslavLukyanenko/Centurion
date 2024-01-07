import {Injectable, NgZone} from "@angular/core";
import {AuthenticationService} from "./authentication.service";
import {SchedulerService} from "./scheduler.service";
import {Observable, Subject, throwError} from "rxjs";
import {TokenService} from "./token.service";

const MAX_TIMEOUT = 2147483647;

@Injectable({
  providedIn: "root"
})
export class AuthenticationSchedulerService {

  constructor(
    private readonly auth: AuthenticationService,
    private readonly scheduler: SchedulerService,
    private readonly token: TokenService,
    private ngZone: NgZone
  ) {
  }

  scheduleTokenRenewal(): Observable<boolean> {
    if (!this.auth.canReauthenticate()) {
      return throwError("Refresh token expired.");
    }
    const renewalSubj = new Subject<boolean>();

    this.ngZone.runOutsideAngular(() => {
      this.scheduler.scheduleAsyncJob(
        "monitors-panel.scheduler.token.renewal",
        () => this.auth.reauthenticate(),
        this.intervalFactory,
        err => {
          this.auth.logOut();
          return err;
        });
    });

    return renewalSubj.asObservable();
  }

  private intervalFactory = () => {
    const now = Date.now();
    const expirationDate = Math.min(this.token.accessTokenExpirationDate.valueOf(), this.token.refreshTokenExpirationDate.valueOf());
    const tokenLifetimePeriod = expirationDate - now;
    const timeBefore = Math.min(30 * 1000, tokenLifetimePeriod / 2); // 30 sec
    return Math.min(tokenLifetimePeriod - timeBefore, MAX_TIMEOUT);
  }
}
