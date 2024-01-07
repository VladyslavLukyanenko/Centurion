import {ChangeDetectionStrategy, Component, OnInit} from "@angular/core";
import {DisposableComponentBase} from "../../shared/components/disposable.component-base";
import {PaymentsService} from "../../dashboards-api";
import {ActivatedRoute, Router} from "@angular/router";

@Component({
  selector: "app-payment-processing",
  template: `
    <div class="Page-header">
      <h1 class="Page-title">Processing payment</h1>
    </div>

    <div class="Page-content">
      <div class="ContentSection">
        Please wait...
        <i class="pi pi-spin pi-spinner"></i>
      </div>
    </div>

  `,
  styles: [``],
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: {
    class: "PaymentProcessingPage Page"
  }
})
export class PaymentProcessingComponent extends DisposableComponentBase implements OnInit {
  constructor(private paymentsService: PaymentsService,
              private router: Router,
              private activatedRoute: ActivatedRoute) {
    super();
  }

  ngOnInit(): void {
    this.activatedRoute.queryParams
      .pipe(this.untilDestroy())
      .subscribe(async q => {
        if (!q.sid) {
          return;
        }

        try {
          await this.asyncTracker.executeAsAsync(
            this.paymentsService.paymentsProcessPayment(q.sid)
          );

          await this.router.navigate(["../success"], {relativeTo: this.activatedRoute, replaceUrl: true});
        } catch (e) {
          await this.router.navigate(["../failure"], {
            relativeTo: this.activatedRoute,
            replaceUrl: true,
            queryParams: {sid: q.sid}
          });
        }
      });
  }
}
