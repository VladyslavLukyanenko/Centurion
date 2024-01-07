import {Injectable} from "@angular/core";
import {BehaviorSubject} from "rxjs";
import {RouteData} from "../models/route-data.model";
import {ActivatedRoute, Router, Routes} from "@angular/router";
import {AppSettingsService} from "./app-settings.service";
import {AuthenticationService} from "./authentication.service";
import {TokenService} from "./token.service";
import {ClaimNames} from "../models/claim-names.model";
import {IdentityService} from "./identity.service";
import {MenuItem, MenuItemsProvider} from "./menu-items.provider";


const lastLoggedInDashboardDomain = "space.dashboards.lastloggedin.domain";
const lastLoggedInDashboardMode = "space.dashboards.lastloggedin.mode";

@Injectable({
  providedIn: "root"
})
export class RoutesProvider {
  private _rootRoutes$ = new BehaviorSubject<RouteData[]>([]);
  private _secondLvlRoutes$ = new BehaviorSubject<RouteData[]>([]);
  rootRoutes$ = this._rootRoutes$.asObservable();
  secondLvlRoutes$ = this._secondLvlRoutes$.asObservable();

  constructor(private tokenService: TokenService, private id: IdentityService, private menuProvider: MenuItemsProvider) {
    tokenService.encodedAccessToken$.subscribe(t => {
      if (t) {
        const token = tokenService.decodedAccessToken;
        localStorage.setItem(lastLoggedInDashboardMode, token[ClaimNames.currentDashboardHostingMode]);
        localStorage.setItem(lastLoggedInDashboardDomain, token[ClaimNames.currentDashboardDomain]);
      }
    });
  }

  setRootRoutes(routes: RouteData[]): void {
    this._rootRoutes$.next(routes);
  }

  setSecondLvlRoutes(routes: RouteData[]): void {
    this._secondLvlRoutes$.next(routes);
  }

  extractRouteDataList(routes: Routes): RouteData[] {
    return routes.map(r => r.data as RouteData).filter(r => r && r instanceof RouteData);
  }

  resolveUrlFromRoot(...segments: string[]): string[] {
    return ["/", ...segments.filter(r => !!r)];
  }

  getLoginUrl(): string[] {
    const m = /\/(\w+)\/login\/?/.exec(location.pathname);
    let domain = m && m[1] || "";
    if (localStorage.getItem(lastLoggedInDashboardMode) === "PathSegment") {
      domain = localStorage.getItem(lastLoggedInDashboardDomain) || "";
    }

    return ["/", domain, "login"].filter(t => !!t);
  }

  getForbiddenUrl(): string[] {
    return ["/forbidden"];
  }

  getAuthenticatedRedirectUrl(): string[] {
    const route = this.getFirstPermittedRoute(this.menuProvider.getRegularMenuItems())
      || this.getFirstPermittedRoute(this.menuProvider.getAdvancedMenuItems())
      || "/not-found";

    return [route];
  }

  private getFirstPermittedRoute(menu: MenuItem[]): string | null {
    for (const r of menu) {
      if (r.permissions && this.id.userHasPermissions(r.permissions)) {
        return r.route;
      }
    }

    return null;
  }

  getProductPurchasePage(): string[] {
    return ["/", "landing", "product"];
  }

  getPurchasesPage(): string[] {
    return ["/purchases"];
  }
}
