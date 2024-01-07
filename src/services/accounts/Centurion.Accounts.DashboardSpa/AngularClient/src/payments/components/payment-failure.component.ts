import {Component, OnInit, ChangeDetectionStrategy} from "@angular/core";
import {RoutesProvider} from "../../core/services/routes.provider";
import {PaymentsService} from "../../dashboards-api";
import {ActivatedRoute} from "@angular/router";
import {DisposableComponentBase} from "../../shared/components/disposable.component-base";

@Component({
  selector: "app-payment-failure",
  template: `
    <div class="Page-header">
      <h1 class="Page-title">Payment failed</h1>
    </div>

    <div class="Page-content">
      <div class="ContentSection" style="color: #e34">
        Unfortunately we can't receive payment from you.
        <a class="ActionButton is-outlined is-primary" [routerLink]="productPage">Back to product page</a>
      </div>
    </div>

  `,
  styles: [``],
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: {
    class: "Page"
  }
})
export class PaymentFailureComponent extends DisposableComponentBase implements OnInit {
  productPage: string[];

  constructor(private routes: RoutesProvider,
              private activatedRoute: ActivatedRoute,
              private paymentsService: PaymentsService) {
    super();
  }

  ngOnInit(): void {
    this.productPage = this.routes.getProductPurchasePage();

    const sid = this.activatedRoute.queryParams
      .pipe(this.untilDestroy())
      .subscribe(async q => {
        if (!q.sid) {
          return;
        }

        await this.paymentsService.paymentsCancelPayment(q.sid).toPromise();
      });
  }
}
