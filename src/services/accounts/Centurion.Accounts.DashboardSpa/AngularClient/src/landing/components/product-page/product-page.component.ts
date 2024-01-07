import {ChangeDetectionStrategy, Component, OnInit} from "@angular/core";
import {DisposableComponentBase} from "../../../shared/components/disposable.component-base";
import {BehaviorSubject} from "rxjs";
import {filter, map} from "rxjs/operators";
import {
  DashboardsService,
  PaymentsService,
  ProductPublicInfoData,
  ReleasesService,
  ReleaseStockData
} from "../../../dashboards-api";
import {ToolbarService} from "../../../core/services/toolbar.service";
import {ActivatedRoute} from "@angular/router";
import {NotificationService} from "../../../core/services/notifications/notification.service";
import {OperationStatusMessage} from "../../../core/services/notifications/messages.model";
import {loadStripe, Stripe} from "@stripe/stripe-js";
import {environment} from "../../../environments/environment";

@Component({
  selector: "app-product-page",
  templateUrl: "./product-page.component.html",
  styleUrls: ["./product-page.component.less"],
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: {
    class: "Page ProductLanding"
  }
})
export class ProductPageComponent extends DisposableComponentBase implements OnInit {

  private pwd: string;
  product$ = new BehaviorSubject<ProductPublicInfoData>(null);
  release$ = new BehaviorSubject<ReleaseStockData>(null);
  isTrialBoughtAlertVisible$ = new BehaviorSubject<boolean>(false);
  private stripePromise: Promise<Stripe>;

  constructor(private dashboardsService: DashboardsService,
              private toolbarService: ToolbarService,
              private releasesService: ReleasesService,
              private paymentsService: PaymentsService,
              private notifications: NotificationService,
              private activatedRoute: ActivatedRoute) {
    super();
  }

  async ngOnInit(): Promise<void> {
    this.stripePromise = loadStripe(environment.stripe.publicKey);
    this.activatedRoute.queryParams
      .pipe(this.untilDestroy())
      .subscribe(async q => {
        this.isTrialBoughtAlertVisible$.next(false);
        await this.refreshReleaseInfo(q.pwd);
      });

    this.product$
      .pipe(
        this.untilDestroy(),
        filter(p => !!p)
      )
      .subscribe(p => this.toolbarService.setTitle(`${p.name} v${p.version}`));
    const data = await this.asyncTracker.executeAsAsync(
      this.dashboardsService.dashboardsGetCurrent().pipe(map(_ => _.payload))
    );

    this.product$.next(data);
  }

  private async refreshReleaseInfo(pwd: string): Promise<void> {
    this.pwd = pwd;
    if (!pwd) {
      this.release$.next(null);
      return;
    }

    try {
      const r = await this.asyncTracker.executeAsAsync(
        this.releasesService.releasesGetStock(pwd).pipe(map(_ => _.payload))
      );

      this.release$.next(r);
    } catch (e) {
      // noop
    }
  }

  async buyTrialOrCreateSession(): Promise<void> {
    const stockData = this.release$.value;
    if (!stockData) {
      return;
    }

    try {
      if (stockData.isTrial) {
        await this.asyncTracker.executeAsAsync(this.paymentsService.paymentsObtainTrial(this.pwd));
        this.isTrialBoughtAlertVisible$.next(true);
      } else {
        const sessionId = await this.asyncTracker.executeAsAsync(
          this.paymentsService.paymentsCreatePayment({password: this.pwd})
            .pipe(map(_ => _.payload))
        );

        const stripe = await this.stripePromise;
        const {error} = await stripe.redirectToCheckout({sessionId});
        if (error) {
          this.notifications.error(error.message);
        }
      }
    } catch (e) {
      this.notifications.error(OperationStatusMessage.FAILED);
    }
  }
}
