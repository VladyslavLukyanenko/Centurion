import {InfiniteScrollDataSourceBase} from "../../core/services/infinite-scroll.data-source-base";
import {ReleaseType, ReleaseData, ReleasesService, ReleasesSortBy, ReleaseRowData} from "../../dashboards-api";
import {Injectable} from "@angular/core";
import {Observable} from "rxjs/internal/Observable";
import {ApiContract} from "../../core/models/api-contract.model";
import {PagedList} from "../../core/models/paged-list.model";
import {BehaviorSubject, combineLatest} from "rxjs";
import {debounceTime, distinctUntilChanged} from "rxjs/operators";
import {CollectionViewer} from "@angular/cdk/collections";

@Injectable()
export class ReleasesDataSource extends InfiniteScrollDataSourceBase<ReleaseRowData> {
  private _searchTerm$ = new BehaviorSubject<string>(null);
  private _planId$ = new BehaviorSubject<number | null>(null);
  private _type$ = new BehaviorSubject<ReleaseType | null>(null);
  private _sortBy$ = new BehaviorSubject<ReleasesSortBy | null>(null);

  planId$ = this._planId$.asObservable()
    .pipe(distinctUntilChanged());

  typeId$ = this._type$.asObservable()
    .pipe(distinctUntilChanged());

  sortBy$ = this._sortBy$.asObservable()
    .pipe(distinctUntilChanged());

  searchTerm$ = this._searchTerm$.asObservable()
    .pipe(distinctUntilChanged(), debounceTime(300))

  constructor(private releasesService: ReleasesService) {
    super(10);
  }


  setPlanId(value: number | null) {
    this._planId$.next(value);
  }

  setType(value: ReleaseType | null) {
    this._type$.next(value in ReleaseType && value || null);
  }

  setSortBy(value: ReleasesSortBy | null) {
    this._sortBy$.next(value in ReleasesSortBy && value || null);
  }

  connect(collectionViewer: CollectionViewer): Observable<ReleaseData[]> {
    combineLatest([this.searchTerm$, this.planId$, this.typeId$, this.sortBy$])
      .pipe(this.untilDisconnect())
      .subscribe(() => {
        this.resetData();
        return this.fetchPageOnDemand(0);
      });


    return super.connect(collectionViewer);
  }

  protected fetchPage(pageIdx: number, pageSize: number): Observable<ApiContract<PagedList<ReleaseData>>> {
    return this.releasesService.releasesGetPage(this._planId$.value, this._type$.value, this._sortBy$.value,
      this._searchTerm$.value, pageIdx, null, pageSize);
  }

}
