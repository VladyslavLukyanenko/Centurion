import {
  ActivatedRoute,
  ActivatedRouteSnapshot,
  CanActivate,
  Router,
  RouterModule,
  RouterStateSnapshot,
  Routes
} from "@angular/router";
import {Injectable, NgModule} from "@angular/core";
import {AnalyticsGeneralComponent} from "./components/analytics-general/analytics-general.component";
import {AnalyticsDiscordComponent} from "./components/analytics-discord/analytics-discord.component";
import {IdentityService} from "../core/services/identity.service";
import {AppPermissions} from "../core/models/permissions.model";


@Injectable({
  providedIn: "root"
})
export class AnalyticsRedirectGuard implements CanActivate {
  constructor(private id: IdentityService,
              private router: Router) {
  }

  canActivate(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Promise<boolean> {
    if (this.id.currentUser.permissions.indexOf(AppPermissions.AnalyticsGeneralRead) !== -1) {
      return this.router.navigate(["analytics/general"], {replaceUrl: true});
    }

    return this.router.navigate(["analytics/discord"], {replaceUrl: true});
  }
}

const routes: Routes = [
  {
    path: "",
    pathMatch: "full",
    children: [],
    canActivate: [AnalyticsRedirectGuard]
  },
  {
    path: "general",
    component: AnalyticsGeneralComponent
  },
  {
    path: "discord",
    component: AnalyticsDiscordComponent
  }
];

@NgModule({
  imports: [
    RouterModule.forChild(routes)
  ],
  providers: [AnalyticsRedirectGuard],
  exports: [
    RouterModule
  ]
})
export class AnalyticsRoutingModule {
}
