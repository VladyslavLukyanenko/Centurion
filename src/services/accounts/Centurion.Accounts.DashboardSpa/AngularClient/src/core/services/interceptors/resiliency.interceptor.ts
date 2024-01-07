import {
  HTTP_INTERCEPTORS,
  HttpErrorResponse,
  HttpEvent,
  HttpEventType,
  HttpHandler,
  HttpInterceptor,
  HttpRequest
} from "@angular/common/http";
import {Observable, Subject, throwError, timer} from "rxjs";
import {catchError, finalize, mergeMap, retryWhen} from "rxjs/operators";
import {AuthenticationService} from "../authentication.service";
import {Injectable, Injector} from "@angular/core";
import {TokenService} from "../token.service";
import {getLogger} from "../logging.service";


const retryStrategy = ({maxRetryAttempts = 5, scalingDuration = 1000, includedStatusCodes = [500, 502, 504]} = {}) => {
  const retryStrategyLogger = getLogger("retryStrategy");
  return (attempts: Observable<any>) =>
    attempts.pipe(
      mergeMap((error, idx) => {
          const currentAttempt = idx + 1;
          if (currentAttempt > maxRetryAttempts) {
            retryStrategyLogger.error("Maximum retry attempts reached");
            return throwError(error);
          } else if (includedStatusCodes.indexOf(error.status) === -1) {
            retryStrategyLogger.debug("Retry attempt skipped.");
            return throwError(error);
          }

          const delay = currentAttempt * scalingDuration;
        retryStrategyLogger.warn(`Retry in ${delay}ms...`);
          return timer(delay);
        }
      )
    );
};

@Injectable({
  providedIn: "root"
})
export class ResiliencyInterceptor implements HttpInterceptor {
  interceptorLogger = getLogger("ResiliencyInterceptor");
  private pendingReauthentication: boolean = false;
  private pendingRequests: Subject<any>[] = [];

  constructor(private injector: Injector) {
  }

  private get authService(): AuthenticationService {
    return this.injector.get(AuthenticationService);
  }

  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    return new Observable<HttpEvent<any>>(observer => {
      next.handle(req)
        .pipe(
          retryWhen(retryStrategy()),
          catchError(e => this.handleError(e, req, next))
        )
        .subscribe(r => {
            observer.next(r);
            if (r.type === HttpEventType.Response) {
              observer.complete();
            }
          },
          e => {
            observer.error(e);
            observer.complete();
          });
    });
  }

  private handleError<T>(error: HttpErrorResponse, httpRequest: HttpRequest<any>, next: HttpHandler) {
    if (error.status === 401) {
      return this.reauthenticate()
        .pipe(
          mergeMap(() => {
            const tokenService = this.injector.get(TokenService);
            const token = tokenService.encodedAccessToken;
            httpRequest = httpRequest.clone({
              setHeaders: {
                "Authorization": "Bearer " + token
              }
            });

            return next.handle(httpRequest);
          }),
        )
        .toPromise();
    }

    return throwError(error);
  }

  reauthenticate(): Observable<any> {
    const newPendingRequest = new Subject<void>();
    this.pendingRequests.push(newPendingRequest);
    this.interceptorLogger.debug("pendingRequests " + this.pendingRequests.length);
    if (!this.pendingReauthentication) {
      this.interceptorLogger.debug("reauthenticate " + (new Date));
      this.pendingReauthentication = true;

      this.authService.reauthenticate()
        .pipe(
          retryWhen(retryStrategy()),
          catchError(error => {
            this.authService.logOut();

            return throwError(error);
          }),
          finalize(() => {
            this.pendingReauthentication = false;
          })
        )
        .subscribe(() => {
          this.interceptorLogger.debug("reauthenticate refreshed " + (new Date));
          this.popPendingRequestAndExecute(pr => pr.next());
          this.interceptorLogger.warn("reauthenticate resumed pending requests " + (new Date));
        }, (e) => {
          this.interceptorLogger.error("reauthenticate error " + (new Date));
          this.popPendingRequestAndExecute(pr => pr.error(e));
        });
    }

    return newPendingRequest;
  }

  private popPendingRequestAndExecute(executor: (r: Subject<void>) => void) {
    while (this.pendingRequests.length) {
      const pr = this.pendingRequests[0];
      executor(pr);
      pr.complete();
      this.pendingRequests.splice(0, 1);
    }

    this.interceptorLogger.debug("popPendingRequestAndExecute " + this.pendingRequests.length);
  }
}

export const ResiliencyInterceptorProvider = {
  multi: true,
  useClass: ResiliencyInterceptor,
  provide: HTTP_INTERCEPTORS
};
