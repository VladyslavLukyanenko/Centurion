import {NgModule} from "@angular/core";
import {EmbedsPageComponent} from "./components/embeds-page/embeds-page.component";
import {SharedModule} from "../shared/shared.module";
import {EmbedsRoutingModule} from "./embeds-routing.module";


@NgModule({
  declarations: [EmbedsPageComponent],
  imports: [
    SharedModule,
    EmbedsRoutingModule
  ]
})
export class EmbedsModule { }
