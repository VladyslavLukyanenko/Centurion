import {AppSettingsService} from "../app-settings.service";
import {Observable} from "rxjs";
import {Injectable} from "@angular/core";
import {
  ActivatedRoute,
  ActivatedRouteSnapshot,
  CanActivate,
  CanActivateChild,
  Router,
  RouterStateSnapshot
} from "@angular/router";
import {AuthenticationService} from "../authentication.service";
import {AuthenticationSchedulerService} from "../authentication-scheduler.service";
import {RoutesProvider} from "../routes.provider";

@Injectable({
  providedIn: "root"
})
export class AuthenticatedGuard implements CanActivate, CanActivateChild {

  constructor(
    private readonly _auth: AuthenticationService,
    private readonly authScheduler: AuthenticationSchedulerService,
    private readonly _router: Router,
    private routesProvider: RoutesProvider,
    private readonly _activatedRoute: ActivatedRoute,
    private readonly _appSettings: AppSettingsService) {
  }

  canActivate(
    route: ActivatedRouteSnapshot,
    state: RouterStateSnapshot): Observable<boolean> | Promise<boolean> | boolean {
    if (!this._auth.isAuthenticated() && !this._auth.canReauthenticate()) {
      this._auth.logOut();
      this._router.routerState.snapshot.url = state.url; // todo: what is it?
      this._router.navigate(this.routesProvider.getLoginUrl());
      return false;
    }

    this.authScheduler.scheduleTokenRenewal();
    return true;
  }

  canActivateChild(childRoute: ActivatedRouteSnapshot, state: RouterStateSnapshot): Observable<boolean> | Promise<boolean> | boolean {
    return this.canActivate(childRoute, state);
  }
}
