import {AppSettingsService} from "../app-settings.service";
import {AuthenticationService} from "../authentication.service";
import {Injectable} from "@angular/core";
import {
  ActivatedRoute,
  ActivatedRouteSnapshot,
  CanActivate,
  CanActivateChild,
  Router,
  RouterStateSnapshot
} from "@angular/router";
import {Observable} from "rxjs/internal/Observable";
import {RoutesProvider} from "../routes.provider";

@Injectable({
  providedIn: "root"
})
export class NotAuthenticatedGuard implements CanActivateChild, CanActivate {
  constructor(
    private readonly _auth: AuthenticationService,
    private readonly _router: Router,
    private routesProvider: RoutesProvider,
    private readonly _appSettings: AppSettingsService,
    private readonly _route: ActivatedRoute,
  ) {
  }

  async canActivateChild(childRoute: ActivatedRouteSnapshot, state: RouterStateSnapshot) {
    const canBeActivated = !this._auth.isAuthenticated() && !this._auth.canReauthenticate();
    if (canBeActivated) {
      return true;
    }

    if (!this._auth.isAuthenticated() && this._auth.canReauthenticate()) {
      await this._auth.reauthenticate().toPromise();
    }

    return await this._router.navigate(this.routesProvider.getAuthenticatedRedirectUrl(), {relativeTo: this._route.root});
  }

  canActivate(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Observable<boolean> | Promise<boolean> | boolean {
    return this.canActivateChild(route, state);
  }
}
