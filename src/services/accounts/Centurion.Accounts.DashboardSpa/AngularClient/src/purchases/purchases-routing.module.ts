import {RouterModule, Routes} from "@angular/router";
import {NgModule} from "@angular/core";
import {PurchasesPageComponent} from "./components/purchases-page/purchases-page.component";

const routes: Routes = [
  {
    path: "",
    component: PurchasesPageComponent
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
export class PurchasesRoutingModule {

}
