import {RouterModule, Routes} from "@angular/router";
import {NgModule} from "@angular/core";
import {LicensesTablePageComponent} from "./components/licenses-table-page/licenses-table-page.component";
import {LicenseManagePageComponent} from "./components/license-manage-page/license-manage-page.component";

const routes: Routes = [
  {
    path: "",
    component: LicensesTablePageComponent
  },
  {
    path: ":id",
    component: LicenseManagePageComponent
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
export class LicensesRoutingModule {
}
