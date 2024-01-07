import {ChangeDetectionStrategy, Component, OnInit} from "@angular/core";
import {BehaviorSubject, combineLatest, Observable} from "rxjs";
import {RoutesProvider} from "../../services/routes.provider";
import {AppSettingsService} from "../../services/app-settings.service";
import {ActivatedRoute, ActivatedRouteSnapshot, NavigationEnd, Router} from "@angular/router";
import {DisposableComponentBase} from "../../../shared/components/disposable.component-base";
import {filter, map} from "rxjs/operators";
import {RouteData} from "../../models/route-data.model";
import {AuthenticationService} from "../../services/authentication.service";
import {IdentityService} from "../../services/identity.service";
import {environment} from "../../../../environments/environment";
import {Title} from "@angular/platform-browser";
import {TranslateService} from "@ngx-translate/core";

@Component({
  selector: "r-layout",
  templateUrl: "./layout.component.html",
  styleUrls: ["./layout.component.scss"],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class LayoutComponent extends DisposableComponentBase implements OnInit {
  rootRoutes$: Observable<MenuItem[]>;
  secondLvlRoutes$: Observable<MenuItem[]>;
  private _navPath = new BehaviorSubject<NavigationSegment[]>([]);

  navPath$ = this._navPath.asObservable();
  isCollapsed: boolean;

  constructor(private routesProvider: RoutesProvider,
              private activatedRoute: ActivatedRoute,
              private router: Router,
              private settings: AppSettingsService,
              private identityService: IdentityService,
              private authService: AuthenticationService,
              private translate: TranslateService,
              private title: Title) {
    super();
  }

  logOut() {
    this.authService.logOut();
    return this.router.navigate(this.routesProvider.getLoginUrl());
  }

  ngOnInit() {
    // todo: move to ToolbarService
    this.router.events
      .pipe(
        this.untilDestroy(),
        filter(e => e instanceof NavigationEnd),
        map(e => <NavigationEnd>e)
      )
      .subscribe(() => this.buildNavSegments());

    this.rootRoutes$ = combineLatest([
      this.routesProvider.rootRoutes$,
    ])
      .pipe(
        this.untilDestroy(),
        map(([routes]) => {
          return routes.map(route => ({
            routeItem: route,
            navSegments: this.routesProvider.resolveUrlFromRoot(route.route)
          }) as MenuItem);
        })
      );

    this.secondLvlRoutes$ = combineLatest([
      this.routesProvider.secondLvlRoutes$,
    ])
      .pipe(
        this.untilDestroy(),
        map(([routes]) => {
          return routes.map(route => ({
            routeItem: route,
            navSegments: this.routesProvider.resolveUrlFromRoot(this.activatedRoute.snapshot.routeConfig.data.route, route.route)
          }) as MenuItem);
        })
      );
    this.buildNavSegments();
  }

  private async buildNavSegments() {
    const dataStack: NavigationSegment[] = [];
    const path: ActivatedRouteSnapshot[] = [];
    let route: ActivatedRouteSnapshot = this.router.routerState.snapshot.root;
    while (route != null) {
      path.push(route);
      route = route.firstChild;
    }

    const segments: string[] = ["/"];
    for (const curr of path) {
      segments.push(...curr.url.map(_ => _.path));
      if (!curr.routeConfig || !(curr.routeConfig.data instanceof RouteData)) {
        continue;
      }

      if (curr.children.length > 1) {
        throw new Error("Not supported list of data on same route level");
      }

      dataStack.push(new NavigationSegment(curr.routeConfig.data, segments.slice()));
    }

    const titleParts = await Promise.all([
      ...dataStack.map(_ => this.translate.get(_.data.title).toPromise()).reverse(),
      Promise.resolve(environment.publicProjectName)
    ]);

    const titleText = titleParts.join(" | ");
    this.title.setTitle(titleText);

    this._navPath.next(dataStack);
  }
}

interface MenuItem {
  routeItem: RouteData;
  navSegments: string[];
}

export class NavigationSegment {
  constructor(readonly data: RouteData, readonly url: string[]) {
  }
}
