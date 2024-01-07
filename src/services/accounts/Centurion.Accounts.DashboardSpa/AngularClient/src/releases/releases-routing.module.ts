import {RouterModule, Routes} from "@angular/router";
import {NgModule} from "@angular/core";
import {ReleasesTablePageComponent} from "./components/releases-table-page/releases-table-page.component";
import {ReleaseManagePageComponent} from "./components/release-manage-page/release-manage-page.component";

const routes: Routes = [
  {
    path: "",
    component: ReleasesTablePageComponent
  },
  {
    path: ":id",
    component: ReleaseManagePageComponent
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
export class ReleasesRoutingModule {

}
