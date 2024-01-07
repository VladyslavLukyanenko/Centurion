import {RouterModule, Routes} from "@angular/router";
import {NgModule} from "@angular/core";
import {StaffMembersListPageComponent} from "./components/staff-members-list-page/staff-members-list-page.component";

const routes: Routes = [
  {
    path: "",
    component: StaffMembersListPageComponent
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
export class StaffRoutingModule {

}
