import {RouterModule, Routes} from "@angular/router";
import {NgModule} from "@angular/core";
import {ProductPageComponent} from "./components/product-page/product-page.component";

const routes: Routes = [
  {
    path: "product",
    component: ProductPageComponent,
    pathMatch: "full"
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
export class LandingRoutingModule {

}
