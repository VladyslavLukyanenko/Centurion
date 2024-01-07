import {ChangeDetectionStrategy, Component, OnDestroy, OnInit} from "@angular/core";
import {BehaviorSubject} from "rxjs";
import {DisposableComponentBase} from "./shared/components/disposable.component-base";
import {ActivityTrackerService} from "./core/services/activity-tracker.service";
import {map} from "rxjs/operators";
import {ActivatedRoute} from "@angular/router";
import {IdentityService} from "./core/services/identity.service";
import {UserSessionService} from "./core/services/user-session.service";
import {getLogger, Logger} from "./core/services/logging.service";
import {MessageService} from "primeng/api";
import {NotificationService} from "./core/services/notifications/notification.service";

const sessionRefreshTimeout = 60 * 1000;
let sessionRefreshTimeId: any;

@Component({
  selector: "app-entry-point",
  templateUrl: "./app.component.html",
  styleUrls: ["./app.component.scss"],
  changeDetection: ChangeDetectionStrategy.OnPush,
  providers: [
    MessageService,
    NotificationService
  ]
})
export class AppComponent extends DisposableComponentBase implements OnInit, OnDestroy {
  isProgressVisible$: BehaviorSubject<boolean> = new BehaviorSubject<boolean>(false);
  logger: Logger;

  constructor(private activityTrackerService: ActivityTrackerService,
              private activatedRoute: ActivatedRoute,
              private userService: IdentityService,
              private sessionService: UserSessionService) {
    super();
    this.logger = getLogger("AppComponent");
  }

  async ngOnInit(): Promise<void> {
    this.activityTrackerService.pendingRequests$
      .pipe(
        this.untilDestroy(),
        map(n => !!n)
      )
      .subscribe(hasActiveRequests => this.isProgressVisible$.next(hasActiveRequests));

    await this.refreshSession();
  }

  private async refreshSession(): Promise<void> {
    try{
      await this.sessionService.refreshSession();
    } catch (e) {
      this.logger.error(e.toString());
    } finally {
      sessionRefreshTimeId = setTimeout(() => this.refreshSession(), sessionRefreshTimeout);
    }
  }


  ngOnDestroy(): void {
    super.ngOnDestroy();
    clearTimeout(sessionRefreshTimeout);
  }
}
