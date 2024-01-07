import {InfiniteScrollDataSourceBase} from "./infinite-scroll.data-source-base";
import {GlobalSearchResult, GlobalSearchService} from "../../dashboards-api";
import {PagedList} from "../models/paged-list.model";
import {ApiContract} from "../models/api-contract.model";
import {Observable} from "rxjs/internal/Observable";
import {BehaviorSubject, combineLatest} from "rxjs";
import {debounceTime, distinctUntilChanged} from "rxjs/operators";
import {Injectable} from "@angular/core";
import {CollectionViewer} from "@angular/cdk/collections";

@Injectable()
export class GlobalSearchDataSource extends InfiniteScrollDataSourceBase<GlobalSearchResult> {
  private _searchTerm$ = new BehaviorSubject<string>(null);

  searchTerm$ = this._searchTerm$.asObservable()
    .pipe(distinctUntilChanged(), debounceTime(300));

  constructor(private globalSearchService: GlobalSearchService) {
    super();
  }

  connect(collectionViewer: CollectionViewer): Observable<GlobalSearchResult[]> {
    combineLatest([this.searchTerm$])
      .pipe(this.untilDisconnect())
      .subscribe(([searchTerm]) => {
        if (searchTerm) {
          this.refreshData();
        } else {
          this.resetData();
        }
      });

    return super.connect(collectionViewer);
  }

  setSearchTerm(value: string | null) {
    this._searchTerm$.next(value?.trim());
  }

  protected fetchPage(pageIdx: number, pageSize: number): Observable<ApiContract<PagedList<GlobalSearchResult>>> {
    return this.globalSearchService.globalSearchGet(this._searchTerm$.value, pageIdx, null, pageSize);
  }

  isFirstChildAt(idx: number): boolean {
    if (idx === 0) {
      return true;
    }

    const prev = this.cache[idx - 1];
    const curr = this.cache[idx];
    return prev && curr && prev.type != curr.type
  }
}
