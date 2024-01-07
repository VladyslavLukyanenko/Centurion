import {ChangeDetectionStrategy, Component, HostListener, OnInit} from "@angular/core";
import {AuthenticationService} from "../../../core/services/authentication.service";
import {environment} from "../../../environments/environment";
import {BehaviorSubject} from "rxjs";
import {map} from "rxjs/operators";
import {DashboardsService} from "../../../dashboards-api";
import {DisposableComponentBase} from "../../../shared/components/disposable.component-base";
import {ToolbarService} from "../../../core/services/toolbar.service";

@Component({
  selector: "app-login",
  templateUrl: "./login.component.html",
  styleUrls: ["./login.component.less"],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class LoginComponent extends DisposableComponentBase implements OnInit {
  private wnd!: Window;
  authorizeUrl$ = new BehaviorSubject<string | null>(null);

  unableAuthenticate$ = this.authorizeUrl$.asObservable()
    .pipe(
      map(u => !u)
    );

  constructor(private authService: AuthenticationService,
              private toolbarService: ToolbarService,
              private dashboardsService: DashboardsService) {
    super();
    toolbarService.setTitle("Login");
  }


  @HostListener("window:storage", ["$event"])
  async storageHandler(e: StorageEvent): Promise<void> {
    if (e.newValue && e.key === environment.auth.discord.codeKey) {
      this.wnd?.close();
      localStorage.removeItem(environment.auth.discord.codeKey);
      try {
        await this.asyncTracker.executeAsAsync(
          this.authService.authenticateWithExternal({code: e.newValue}));
      } catch (e) {
        alert(e);
        return;
      }
    }
  }

  async ngOnInit(): Promise<void> {
    const r = await this.asyncTracker.executeAsAsync(
      this.dashboardsService.dashboardsGetLoginData(location.toString()));
    this.authorizeUrl$.next(r.payload?.discordAuthorizeUrl);
  }

  authenticate(): void {
    const url = this.authorizeUrl$.value;
    this.wnd = window.open(url, "", "height=400,width=600");
  }

}
