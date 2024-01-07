import {RouterModule, Routes} from "@angular/router";
import {NgModule} from "@angular/core";
import {FormsPageComponent} from "./components/forms-page/forms-page.component";

const routes: Routes = [
  {
    path: "",
    component: FormsPageComponent
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
export class FormsRoutingModule {

}
