import {Injectable} from "@angular/core";
import {InfiniteScrollDataSourceBase} from "../../core/services/infinite-scroll.data-source-base";
import {LicenseKeySnapshotData, LicenseKeysService, LicensesSortBy} from "../../dashboards-api";
import {BehaviorSubject, combineLatest} from "rxjs";
import {debounceTime, distinctUntilChanged} from "rxjs/operators";
import {CollectionViewer} from "@angular/cdk/collections";
import {Observable} from "rxjs/internal/Observable";
import {ApiContract} from "../../core/models/api-contract.model";
import {PagedList} from "../../core/models/paged-list.model";

@Injectable()
export class LicensesDataSource extends InfiniteScrollDataSourceBase<LicenseKeySnapshotData> {
  private _searchTerm$ = new BehaviorSubject<string>(null);
  private _isLifetimeOnly$ = new BehaviorSubject<boolean | null>(null);
  private _planId$ = new BehaviorSubject<number | null>(null);
  private _releaseId$ = new BehaviorSubject<number | null>(null);
  private _sortBy$ = new BehaviorSubject<LicensesSortBy | null>(null);

  searchTerm$ = this._searchTerm$.asObservable()
    .pipe(distinctUntilChanged(), debounceTime(300));

  isLifetimeOnly$ = this._isLifetimeOnly$.asObservable()
    .pipe(distinctUntilChanged());

  planId$ = this._planId$.asObservable()
    .pipe(distinctUntilChanged());

  releaseId$ = this._releaseId$.asObservable()
    .pipe(distinctUntilChanged());

  sortBy$ = this._sortBy$.asObservable()
    .pipe(distinctUntilChanged());


  constructor(private licenseKeyService: LicenseKeysService) {
    super();
  }

  setLifetimeOnly(value: boolean | null): void {
    this._isLifetimeOnly$.next(value);
  }

  setPlanId(value: number | null): void {
    this._planId$.next(value);
  }

  setReleaseId(value: number | null): void {
    this._releaseId$.next(value);
  }

  setSortBy(value: LicensesSortBy | null): void {
    this._sortBy$.next(value in LicensesSortBy && value || null);
  }

  connect(collectionViewer: CollectionViewer): Observable<LicenseKeySnapshotData[]> {
    combineLatest([this.searchTerm$, this.isLifetimeOnly$, this.planId$, this.releaseId$, this.sortBy$])
      .pipe(this.untilDisconnect())
      .subscribe(() => {
        return this.refreshData();
      });


    return super.connect(collectionViewer);
  }

  protected fetchPage(pageIdx: number, pageSize: number): Observable<ApiContract<PagedList<LicenseKeySnapshotData>>> {
    return this.licenseKeyService
      .licenseKeysGetLicenseKeys(this._isLifetimeOnly$.value, this._planId$.value, this._releaseId$.value,
        this._sortBy$.value, this._searchTerm$.value, pageIdx, null, pageSize);
  }
}
