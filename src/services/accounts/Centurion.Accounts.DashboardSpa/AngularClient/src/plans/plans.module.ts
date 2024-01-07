import {NgModule} from "@angular/core";
import {SharedModule} from "../shared/shared.module";
import {PlansPageComponent} from "./components/plans-page/plans-page.component";
import {PlansRoutingModule} from "./plans-routing.module";
import { PlanEditDialogComponent } from "./components/plan-edit-dialog/plan-edit-dialog.component";


@NgModule({
  declarations: [PlansPageComponent, PlanEditDialogComponent],
  imports: [
    SharedModule,
    PlansRoutingModule
  ]
})
export class PlansModule { }
