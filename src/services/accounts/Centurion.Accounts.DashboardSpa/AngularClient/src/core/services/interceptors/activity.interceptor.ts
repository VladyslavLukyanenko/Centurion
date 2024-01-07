import {Injectable} from "@angular/core";
import {HTTP_INTERCEPTORS, HttpEvent, HttpHandler, HttpInterceptor, HttpRequest} from "@angular/common/http";
import {Observable} from "rxjs";
import {ActivityTrackerService} from "../activity-tracker.service";
import {finalize} from "rxjs/operators";

@Injectable({
  providedIn: "root"
})
export class ActivityInterceptor implements HttpInterceptor {
  constructor(private activityService: ActivityTrackerService) {
  }

  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    this.activityService.push(req);
    return next.handle(req)
      .pipe(finalize(() => this.activityService.remove(req)));
  }

}

export const ActivityInterceptorProvider = {
  useClass: ActivityInterceptor,
  provide: HTTP_INTERCEPTORS,
  multi: true
};
