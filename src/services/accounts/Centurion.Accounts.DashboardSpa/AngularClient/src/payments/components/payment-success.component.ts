import {Component, OnInit, ChangeDetectionStrategy} from "@angular/core";
import {RoutesProvider} from "../../core/services/routes.provider";

@Component({
  selector: "app-payment-success",
  template: `
    <div class="Page-header">
      <h1 class="Page-title">Payment received</h1>
    </div>

    <div class="Page-content">
      <div class="ContentSection">
        Thank you. Payment was received and processed!
        <a class="ActionButton is-primary" [routerLink]="purchasesPage">Go to purchases</a>
      </div>
    </div>

  `,
  styles: [``],
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: {
    class: "Page"
  }
})
export class PaymentSuccessComponent implements OnInit {
  purchasesPage: string[];

  constructor(private routes: RoutesProvider) {
  }

  ngOnInit(): void {
    this.purchasesPage = this.routes.getPurchasesPage();
  }
}
