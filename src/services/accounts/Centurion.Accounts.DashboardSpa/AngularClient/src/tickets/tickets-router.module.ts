import {RouterModule, Routes} from "@angular/router";
import {NgModule} from "@angular/core";
import {TicketsPageComponent} from "./components/tickets-page/tickets-page.component";

const routes: Routes = [
  {
    path: "",
    component: TicketsPageComponent
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
export class TicketsRouterModule {

}
