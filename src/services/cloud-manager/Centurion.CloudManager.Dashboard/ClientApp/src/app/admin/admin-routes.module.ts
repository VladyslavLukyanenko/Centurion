import {RouterModule, Routes} from "@angular/router";
import {RouteData} from "../core/models/route-data.model";
import {NgModule} from "@angular/core";
import {SecondLvlRoutesGuard} from "../core/services/guards/second-lvl.routes-guard";
import {DashboardPageComponent} from "./components/dashboard-page/dashboard-page.component";
import {ServerImagesPageComponent} from "./components/server-images-page/server-images-page.component";
import {ComponentStatsPageComponent} from "./components/component-stats-page/component-stats-page.component";

const routes: Routes = [
  {
    path: "",
    canActivate: [SecondLvlRoutesGuard],
    runGuardsAndResolvers: "always",
    children: [
      {
        path: "",
        pathMatch: "full",
        redirectTo: "dashboard"
      },
      {
        path: "dashboard",
        component: DashboardPageComponent,
        data: new RouteData("dashboard", "admin.dashboard.title", "dashboard")
      },
      {
        path: "component-stats",
        component: ComponentStatsPageComponent,
        data: new RouteData("component-stats", "admin.component-stats.title", "api")
      },
      {
        path: "server-images",
        component: ServerImagesPageComponent,
        data: new RouteData("server-images", "admin.serverImages.title", "deployment-unit")
      }
    ]
  }
];

@NgModule({
  imports: [
    RouterModule.forChild(routes)
  ],
  exports: [
    RouterModule
  ]
})
export class AdminRoutesModule {
}
