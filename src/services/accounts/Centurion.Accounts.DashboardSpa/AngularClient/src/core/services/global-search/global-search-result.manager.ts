import {GlobalSearchResult, SearchResultType} from "../../../dashboards-api";
import {Injectable, InjectionToken} from "@angular/core";
import {ActivatedRoute, Router} from "@angular/router";
import {DateUtil} from "../date.util";

export interface GlobalSearchResultManager {
  supports(type: string): boolean;

  navigateToItemPage(item: GlobalSearchResult): Promise<any>;

  getFormattedDetails(item: GlobalSearchResult): string;

  getGroupLabelFor(r: GlobalSearchResult): string;
}

abstract class GlobalSearchResultManagerBase implements GlobalSearchResultManager {
  protected constructor(private type: string,
                        private router: Router,
                        private activatedRoute: ActivatedRoute) {
  }

  async navigateToItemPage(item: GlobalSearchResult): Promise<any> {
    await this.router.navigate(this.getResultItemPageUrl(item), {relativeTo: this.activatedRoute.root});
  }

  supports(type: string): boolean {
    return type === this.type;
  }


  abstract getFormattedDetails(item: GlobalSearchResult): string;

  abstract getGroupLabelFor(r: GlobalSearchResult): string;

  protected abstract getResultItemPageUrl(item: GlobalSearchResult): string[];
}


@Injectable()
export class LicenseKeySearchClickHandler extends GlobalSearchResultManagerBase {
  constructor(router: Router,
              activatedRoute: ActivatedRoute) {
    super(SearchResultType.LicenseKey, router, activatedRoute);
  }

  getFormattedDetails(item: GlobalSearchResult): string {
    return item.details
      ? `Binded to ${item.details}`
      : "<Not bound yet>";
  }

  getGroupLabelFor(r: GlobalSearchResult): string {
    return "Licenses";
  }

  protected getResultItemPageUrl(item: GlobalSearchResult): string[] {
    return ["licenses", item.id];
  }
}

@Injectable()
export class ReleasesKeySearchClickHandler extends GlobalSearchResultManagerBase {
  constructor(router: Router,
              activatedRoute: ActivatedRoute) {
    super(SearchResultType.Release, router, activatedRoute);
  }

  getFormattedDetails(item: GlobalSearchResult): string {
    return `Created on ${DateUtil.humanizeDate(item.details)}`
  }

  getGroupLabelFor(r: GlobalSearchResult): string {
    return "Releases";
  }

  protected getResultItemPageUrl(item: GlobalSearchResult): string[] {
    return ["releases", item.id];
  }
}

export const GLOBAL_SEARCH_RESULT_MANAGER = new InjectionToken("GlobalSearchResultManager");
