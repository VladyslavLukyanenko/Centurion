import {InfiniteScrollDataSourceBase} from "../../core/services/infinite-scroll.data-source-base";
import {LicenseKeyShortData, LicenseKeysService, ReleaseData} from "../../dashboards-api";
import {Observable} from "rxjs/internal/Observable";
import {ApiContract} from "../../core/models/api-contract.model";
import {PagedList} from "../../core/models/paged-list.model";
import {Injectable} from "@angular/core";
import {BehaviorSubject, combineLatest, of} from "rxjs";
import {CollectionViewer} from "@angular/cdk/collections";
import {distinctUntilChanged, map} from "rxjs/operators";

@Injectable()
export class ReleaseKeysDataSource extends InfiniteScrollDataSourceBase<LicenseKeyShortData> {
  private _releaseId$ = new BehaviorSubject<number>(null);

  releaseId$ = this._releaseId$.asObservable().pipe(distinctUntilChanged());
  totalElements$ = this.lastPage.pipe(map(_ => _?.totalElements ?? 0));

  constructor(private licenseKeysService: LicenseKeysService) {
    super();
  }


  connect(collectionViewer: CollectionViewer): Observable<ReleaseData[]> {
    combineLatest([this.releaseId$])
      .pipe(this.untilDisconnect())
      .subscribe(() => {
        this.resetData();
        return this.fetchPageOnDemand(0);
      });


    return super.connect(collectionViewer);
  }

  setReleaseId(id: number) {
    this._releaseId$.next(id);
  }

  protected fetchPage(pageIdx: number, pageSize: number): Observable<ApiContract<PagedList<LicenseKeyShortData>>> {
    if (!this._releaseId$.value) {
      return of({
        payload: {
          count: 0,
          isEmpty: true,
          content: [],
          isFirst: true,
          isLast: true,
          pageIndex: pageIdx,
          limit: pageSize,
          totalElements: 0,
          totalPages: 0
        }
      });
    }

    return this.licenseKeysService.licenseKeysGetPageByReleaseId(this._releaseId$.value, pageIdx, null, pageSize);
  }
}
