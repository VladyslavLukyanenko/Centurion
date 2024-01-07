import {NgModule} from "@angular/core";
import {SharedModule} from "../shared/shared.module";
import {ProductPageComponent} from "./components/product-page/product-page.component";
import {LandingRoutingModule} from "./landing-routing.module";


@NgModule({
  declarations: [ProductPageComponent],
  imports: [
    SharedModule,
    LandingRoutingModule
  ]
})
export class LandingModule {
}
