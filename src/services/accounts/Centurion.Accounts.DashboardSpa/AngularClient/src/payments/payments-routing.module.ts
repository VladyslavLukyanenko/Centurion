import {RouterModule, Routes} from "@angular/router";
import {NgModule} from "@angular/core";
import {PaymentProcessingComponent} from "./components/payment-processing.component";
import {PaymentSuccessComponent} from "./components/payment-success.component";
import {PaymentFailureComponent} from "./components/payment-failure.component";

const routes: Routes = [
  {
    path: "processing",
    component: PaymentProcessingComponent
  },
  {
    path: "success",
    component: PaymentSuccessComponent
  },
  {
    path: "failure",
    component: PaymentFailureComponent
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
export class PaymentsRoutingModule {

}
