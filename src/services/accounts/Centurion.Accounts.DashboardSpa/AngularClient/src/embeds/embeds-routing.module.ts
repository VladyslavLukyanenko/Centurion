import {RouterModule, Routes} from "@angular/router";
import {NgModule} from "@angular/core";
import {EmbedsPageComponent} from "./components/embeds-page/embeds-page.component";

const routes: Routes = [
  {
    path: "",
    component: EmbedsPageComponent
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
export class EmbedsRoutingModule {

}
