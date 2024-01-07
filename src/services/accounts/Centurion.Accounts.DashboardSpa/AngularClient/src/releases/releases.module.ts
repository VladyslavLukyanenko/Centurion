import {NgModule} from "@angular/core";
import {ReleasesTablePageComponent} from "./components/releases-table-page/releases-table-page.component";
import {SharedModule} from "../shared/shared.module";
import {ReleasesRoutingModule} from "./releases-routing.module";
import { ReleaseEditDialogComponent } from "./components/release-edit-dialog/release-edit-dialog.component";
import { ReleaseManagePageComponent } from "./components/release-manage-page/release-manage-page.component";


@NgModule({
  declarations: [ReleasesTablePageComponent, ReleaseEditDialogComponent, ReleaseManagePageComponent],
  imports: [
    SharedModule,
    ReleasesRoutingModule
  ]
})
export class ReleasesModule { }
