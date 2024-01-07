import {NgModule} from "@angular/core";
import {SharedModule} from "../shared/shared.module";
import {FormsPageComponent} from "./components/forms-page/forms-page.component";
import {FormsRoutingModule} from "./forms-routing.module";


@NgModule({
  declarations: [FormsPageComponent],
  imports: [
    SharedModule,
    FormsRoutingModule
  ]
})
export class FormsModule { }
