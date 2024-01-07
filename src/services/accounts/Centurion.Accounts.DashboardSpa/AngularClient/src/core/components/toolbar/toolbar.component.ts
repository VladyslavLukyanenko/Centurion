import {ChangeDetectionStrategy, Component, EventEmitter, Inject, Input, OnInit, Output} from "@angular/core";
import {AuthenticationService} from "../../services/authentication.service";
import {GlobalSearchDataSource} from "../../services/global-search.data-source";
import {DisposableComponentBase} from "../../../shared/components/disposable.component-base";
import {OverlayPanel} from "primeng/overlaypanel";
import {GlobalSearchResult} from "../../../dashboards-api";
import {
  GLOBAL_SEARCH_RESULT_MANAGER,
  GlobalSearchResultManager
} from "../../services/global-search/global-search-result.manager";
import {AppPermissions} from "../../models/permissions.model";
import {RoutesProvider} from "../../services/routes.provider";

@Component({
  selector: "app-toolbar",
  templateUrl: "./toolbar.component.html",
  styleUrls: ["./toolbar.component.scss"],
  changeDetection: ChangeDetectionStrategy.OnPush,
  providers: [
    GlobalSearchDataSource
  ]
})
export class ToolbarComponent extends DisposableComponentBase implements OnInit {
  @Input()
  sidebarOpened!: boolean;

  @Output()
  sidebarOpenedChange = new EventEmitter<boolean>();

  appPermissions = AppPermissions;
  productPageRoute: string[];

  constructor(private authenticationService: AuthenticationService,
              readonly globalSearchDataSource: GlobalSearchDataSource,
              private routes: RoutesProvider,
              @Inject(GLOBAL_SEARCH_RESULT_MANAGER) private handlers: GlobalSearchResultManager[]) {
    super();
  }

  ngOnInit(): void {
    this.productPageRoute = this.routes.getProductPurchasePage();
  }

  logOut(): void {
    this.authenticationService.logOut();
  }

  setSearchTerm(e: Event, v: string, autosuggestionPanel: OverlayPanel): void {
    this.globalSearchDataSource.setSearchTerm(v);
    if (v?.trim()) {
      autosuggestionPanel.show(e);
    } else {
      autosuggestionPanel.hide();
    }
  }

  getGroupLabelFor(r: GlobalSearchResult): string {
    return this.getManager(r).getGroupLabelFor(r);
  }

  getFormattedDetails(r: GlobalSearchResult): string {
    return this.getManager(r).getFormattedDetails(r);
  }

  navigateToItemPage(r: GlobalSearchResult): Promise<any> {
    return this.getManager(r).navigateToItemPage(r);
  }

  private getManager(item: GlobalSearchResult): GlobalSearchResultManager {
    return this.handlers.find(_ => _.supports(item.type));
  }
}
