import {RouterModule, Routes} from "@angular/router";
import {NgModule} from "@angular/core";
import {RoutesProvider} from "./core/services/routes.provider";
import {HostComponent} from "./host.component";
import {AuthenticatedGuard, NotAuthenticatedGuard} from "./core/services/guards";
import {LayoutComponent} from "./core/components/layout/layout.component";
import {RouteData} from "./core/models/route-data.model";
import {SecondLvlRoutesGuard} from "./core/services/guards/second-lvl.routes-guard";

export const appRoutes: Routes = [
  {
    path: "",
    component: HostComponent,
    runGuardsAndResolvers: "always",
    children: [
      {path: "", redirectTo: "account/login", pathMatch: "full"},
      {
        path: "account",
        canActivateChild: [NotAuthenticatedGuard],
        loadChildren: () => import("./account/account.module").then(_ => _.AccountModule),
      },
      {
        path: "admin",
        component: LayoutComponent,
        canActivateChild: [AuthenticatedGuard],
        runGuardsAndResolvers: "always",
        loadChildren: () => import("./admin/admin.module").then(_ => _.AdminModule),
        data: new RouteData("admin", "admin.title", null)
      }
    ]
  }
];


@NgModule({
  imports: [
    RouterModule.forRoot(appRoutes, {onSameUrlNavigation: "reload"})
  ],
  exports: [
    RouterModule
  ]
})
export class AppRoutingModule {
  constructor(routerProvider: RoutesProvider) {
    const routeData = routerProvider.extractRouteDataList(appRoutes[0].children);
    routerProvider.setRootRoutes(routeData);
  }
}
