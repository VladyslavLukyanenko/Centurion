import {ActivatedRouteSnapshot, CanActivate, CanActivateChild, Router, RouterStateSnapshot} from "@angular/router";
import {Injectable} from "@angular/core";
import {extractDeclaredPermissions} from "../../decorators/permission.decorator";
import {IdentityService} from "../identity.service";
import {RoutesProvider} from "../routes.provider";
import {Location} from "@angular/common";

@Injectable({
  providedIn: "root"
})
export class PermissionsGuard implements CanActivate, CanActivateChild {
  constructor(private id: IdentityService,
              private routes: RoutesProvider,
              private router: Router,
              private location: Location) {
  }

  async canActivate(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Promise<boolean> {
    if (route.component) {
      const permitted = this.id.userHasPermissions({all: extractDeclaredPermissions(route.component)});
      if (!permitted) {
        const forbiddenUrl = this.routes.getForbiddenUrl();
        await this.router.navigate(forbiddenUrl, {skipLocationChange: true, replaceUrl: true});
        this.location.replaceState(state.url);
      }
    }

    return true;
  }

  canActivateChild(childRoute: ActivatedRouteSnapshot, state: RouterStateSnapshot): Promise<boolean> | boolean {
    return this.canActivate(childRoute, state);
  }
}
