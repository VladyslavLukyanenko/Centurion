import {Injectable} from "@angular/core";
import {BehaviorSubject} from "rxjs";
import {RouteData} from "../models/route-data.model";
import {Router, Routes} from "@angular/router";
import {AppSettingsService} from "./app-settings.service";
import {environment} from "../../../environments/environment";

@Injectable({
  providedIn: "root"
})
export class RoutesProvider {
  private _rootRoutes$ = new BehaviorSubject<RouteData[]>([]);
  private _secondLvlRoutes$ = new BehaviorSubject<RouteData[]>([]);
  rootRoutes$ = this._rootRoutes$.asObservable();
  secondLvlRoutes$ = this._secondLvlRoutes$.asObservable();

  constructor(private settings: AppSettingsService, private router: Router) {
  }

  setRootRoutes(routes: RouteData[]) {
    this._rootRoutes$.next(routes);
  }

  setSecondLvlRoutes(routes: RouteData[]) {
    this._secondLvlRoutes$.next(routes);
  }

  extractRouteDataList(routes: Routes): RouteData[] {
    return routes.map(r => <RouteData>r.data).filter(r => r && r instanceof RouteData);
  }

  resolveUrlFromRoot(...segments: string[]) {
    return  ["/", ...segments.filter(r => !!r)];
  }

  getLoginUrl() {
    return [environment.auth.loginRoute];
  }

  getAuthenticatedRedirectUrl() {
    return  [
      "/",
      environment.auth.redirectAuthenticatedRoute
    ];
  }
}
