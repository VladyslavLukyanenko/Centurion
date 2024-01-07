import {NgModule} from "@angular/core";
import {SharedModule} from "../shared/shared.module";
import {StaffRoutingModule} from "./staff-routing.module";
import { StaffMembersListPageComponent } from "./components/staff-members-list-page/staff-members-list-page.component";
import { StaffMemberCardComponent } from "./components/staff-member-card/staff-member-card.component";
import { RolesEditDialogComponent } from "./components/roles-edit-dialog/roles-edit-dialog.component";
import { StaffEditDialogComponent } from "./components/staff-edit-dialog/staff-edit-dialog.component";


@NgModule({
  declarations: [StaffMembersListPageComponent, StaffMemberCardComponent, RolesEditDialogComponent, StaffEditDialogComponent],
  imports: [
    SharedModule,
    StaffRoutingModule
  ]
})
export class StaffModule { }
