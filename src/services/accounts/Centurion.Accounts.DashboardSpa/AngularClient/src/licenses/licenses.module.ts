import {NgModule} from "@angular/core";
import {LicensesTablePageComponent} from "./components/licenses-table-page/licenses-table-page.component";
import {SharedModule} from "../shared/shared.module";
import {LicensesRoutingModule} from "./licenses-routing.module";
import { LicensesCreateDialogComponent } from "./components/licenses-create-dialog/licenses-create-dialog.component";
import { LicenseManagePageComponent } from "./components/license-manage-page/license-manage-page.component";
import { AddExtraDaysDialogComponent } from "./components/add-extra-days-dialog/add-extra-days-dialog.component";
import { BindLicenseKeyToUserDialogComponent } from "./components/bind-license-key-to-user-dialog/bind-license-key-to-user-dialog.component";


@NgModule({
  declarations: [LicensesTablePageComponent, LicensesCreateDialogComponent, LicenseManagePageComponent, AddExtraDaysDialogComponent, BindLicenseKeyToUserDialogComponent],
  imports: [
    SharedModule,
    LicensesRoutingModule
  ]
})
export class LicensesModule { }
