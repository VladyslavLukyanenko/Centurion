import {RouterModule, Routes} from "@angular/router";
import {NgModule} from "@angular/core";
import {SettingsPageComponent} from "./components/settings-page/settings-page.component";
import {OnlyDashboardOwnerGuard} from "../core/services/guards/only-dashboard-owner.guard";


const routes: Routes = [
  {
    path: "",
    component: SettingsPageComponent,
    canActivate: [OnlyDashboardOwnerGuard]
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
export class SettingsRoutingModule {
}
