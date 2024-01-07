import {ActivatedRouteSnapshot, CanActivate, CanActivateChild} from "@angular/router";
import {RoutesProvider} from "../routes.provider";
import {Injectable} from "@angular/core";

@Injectable({
  providedIn: "root"
})
export class SecondLvlRoutesGuard implements CanActivate, CanActivateChild {
  // private readonly routeData: RouteData[];
  constructor(private routesProvider: RoutesProvider) {
  }

  canActivate(activatedRoute: ActivatedRouteSnapshot): boolean {
    const routeData = this.routesProvider.extractRouteDataList(activatedRoute.routeConfig.children);
    this.routesProvider.setSecondLvlRoutes(routeData);
    return true;
  }

  canActivateChild(activatedRoute: ActivatedRouteSnapshot): boolean {
    return this.canActivate(activatedRoute);
  }
}
