import {Injectable} from "@angular/core";
import {ActivatedRouteSnapshot, CanActivate, CanActivateChild, Router, RouterStateSnapshot} from "@angular/router";
import {IdentityService} from "../identity.service";
import {RoutesProvider} from "../routes.provider";
import {Location} from "@angular/common";

@Injectable({
  providedIn: "root"
})
export class OnlyDashboardOwnerGuard implements CanActivate, CanActivateChild {
  constructor(private id: IdentityService,
              private routes: RoutesProvider,
              private router: Router,
              private location: Location) {
  }

  async canActivate(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Promise<boolean> {
    if (this.id.currentUser.isCurrentDashboardOwner) {
      return true;
    }

    const forbiddenUrl = this.routes.getForbiddenUrl();
    await this.router.navigate(forbiddenUrl, {skipLocationChange: true, replaceUrl: true});
    this.location.replaceState(state.url);
    return true;
  }

  canActivateChild(childRoute: ActivatedRouteSnapshot, state: RouterStateSnapshot): Promise<boolean> {
    return this.canActivate(childRoute, state);
  }
}
