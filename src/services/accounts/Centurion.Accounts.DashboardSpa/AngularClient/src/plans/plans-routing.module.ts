import {RouterModule, Routes} from "@angular/router";
import {NgModule} from "@angular/core";
import {PlansPageComponent} from "./components/plans-page/plans-page.component";

const routes: Routes = [
  {
    path: "",
    component: PlansPageComponent
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
export class PlansRoutingModule {

}
