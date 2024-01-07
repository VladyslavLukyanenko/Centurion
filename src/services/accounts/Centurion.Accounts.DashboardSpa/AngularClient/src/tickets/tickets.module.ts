import {NgModule} from '@angular/core';
import {SharedModule} from "../shared/shared.module";
import {TicketsRouterModule} from "./tickets-router.module";
import { TicketsPageComponent } from './components/tickets-page/tickets-page.component';


@NgModule({
  declarations: [TicketsPageComponent],
  imports: [
    SharedModule,
    TicketsRouterModule
  ]
})
export class TicketsModule { }
