import {NgModule} from "@angular/core";
import {SharedModule} from "../shared/shared.module";
import {PaymentProcessingComponent} from "./components/payment-processing.component";
import {PaymentFailureComponent} from "./components/payment-failure.component";
import {PaymentSuccessComponent} from "./components/payment-success.component";
import {PaymentsRoutingModule} from "./payments-routing.module";


@NgModule({
  declarations: [PaymentProcessingComponent, PaymentSuccessComponent, PaymentFailureComponent],
  imports: [
    SharedModule,
    PaymentsRoutingModule
  ]
})
export class PaymentsModule {
}
