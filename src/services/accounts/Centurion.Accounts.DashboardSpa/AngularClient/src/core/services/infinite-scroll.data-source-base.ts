import {CollectionViewer, DataSource} from "@angular/cdk/collections";
import {BehaviorSubject, combineLatest, MonoTypeOperatorFunction, Subject} from "rxjs";
import {AsyncProgressTracker} from "../../shared/models/async-progress-tracker.model";
import {debounceTime, map, takeUntil} from "rxjs/operators";
import {Observable} from "rxjs/internal/Observable";
import {ApiContract} from "../models/api-contract.model";
import {PagedList} from "../models/paged-list.model";
import {ListRange} from "@angular/cdk/collections/collection-viewer";

export abstract class InfiniteScrollDataSourceBase<T> extends DataSource<T> {
  protected cache = Array.from<T>({length: 0});
  private fetchedPages = new Set<number>();
  private data$ = new BehaviorSubject<T[]>(this.cache);
  private disconnect$ = new Subject<void>();
  protected asyncTracker = new AsyncProgressTracker();
  private _lastPage$ = new BehaviorSubject<PagedList<T>>(null);

  protected lastPage = this._lastPage$.asObservable();

  readonly isLoading$ = this.asyncTracker.isLoading$;
  readonly noData$ = combineLatest([this.data$, this.isLoading$])
    .pipe(
      map(([r, l]) => !r.length && !l)
    );

  private fetchExecutor$ = new Subject<ListRange>();
  protected constructor(protected readonly pageSize = 50) {
    super();
  }

  connect(collectionViewer: CollectionViewer): Observable<T[]> {
    collectionViewer.viewChange
      .pipe(this.untilDisconnect())
      .subscribe(range => {
        this.fetchExecutor$.next(range);
      });

    this.fetchExecutor$
      .pipe(
        this.untilDisconnect(),
        debounceTime(50)
      )
      .subscribe(range => {
        const startPage = this.getPageForIndex(range.start);
        const endPage = this.getPageForIndex(range.end - 1);
        for (let i = startPage; i <= endPage; i++) {
          this.fetchPageOnDemand(i);
        }
      });

    return this.data$;
  }

  disconnect(): void {
    this.disconnect$.next();
  }

  refreshData(): Promise<void> {
    this.resetData();
    return this.fetchPageOnDemand(0);
  }

  protected abstract fetchPage(pageIdx: number, pageSize: number): Observable<ApiContract<PagedList<T>>>;

  protected untilDisconnect<TCmp>(): MonoTypeOperatorFunction<TCmp> {
    return takeUntil(this.disconnect$);
  }

  protected resetData(): void {
    this.data$.next([]);
    this.fetchedPages.clear();
    this.cache = Array.from<T>({length: 0});
  }

  protected async fetchPageOnDemand(pageIdx: number): Promise<void> {
    if (this.fetchedPages.has(pageIdx)) {
      return;
    }

    this.fetchedPages.add(pageIdx);
    const pageResponse = await this.asyncTracker.executeAsAsync(this.fetchPage(pageIdx, this.pageSize));

    const page = pageResponse.payload;
    if (page.totalElements !== this.cache.length) {
      this.fetchedPages.clear();
      this.fetchedPages.add(pageIdx);
      this.cache = Array.from<T>({length: page.totalElements});
    }

    const start = pageIdx * this.pageSize;
    this.cache.splice(start, this.pageSize, ...page.content);
    this.data$.next(this.cache);
    this._lastPage$.next(page);
  }

  private getPageForIndex(index: number): number {
    return Math.floor(index / this.pageSize);
  }
}
