import {InfiniteScrollDataSourceBase} from "../services/infinite-scroll.data-source-base";
import {MemberRoleBindingsService, StaffMemberData} from "../../dashboards-api";
import {Observable} from "rxjs/internal/Observable";
import {ApiContract} from "../models/api-contract.model";
import {PagedList} from "../models/paged-list.model";
import {BehaviorSubject, combineLatest} from "rxjs";
import {debounceTime, distinctUntilChanged} from "rxjs/operators";
import {CollectionViewer} from "@angular/cdk/collections";
import {Injectable} from "@angular/core";

@Injectable()
export class MembersDataSource extends InfiniteScrollDataSourceBase<StaffMemberData> {
  private _includeStaff$ = new BehaviorSubject<boolean | null>(null);
  private _searchTerm$ = new BehaviorSubject<string>(null);

  searchTerm$ = this._searchTerm$.asObservable()
    .pipe(distinctUntilChanged(), debounceTime(300));

  includeStaff$ = this._includeStaff$.asObservable()
    .pipe(distinctUntilChanged());

  constructor(private membersService: MemberRoleBindingsService) {
    super();
  }

  setIncludeStaff(value: boolean | null): void {
    this._includeStaff$.next(value);
  }

  setSearchTerm(value: string | null): void {
    this._searchTerm$.next(value);
  }

  connect(collectionViewer: CollectionViewer): Observable<StaffMemberData[]> {
    combineLatest([this.searchTerm$, this.includeStaff$])
      .pipe(this.untilDisconnect())
      .subscribe(() => {
        return this.refreshData();
      });

    return super.connect(collectionViewer);
  }

  protected fetchPage(pageIdx: number, pageSize: number): Observable<ApiContract<PagedList<StaffMemberData>>> {
    return this.membersService.memberRoleBindingsGetStaffMembersPage(this._includeStaff$.value, this._searchTerm$.value,
      pageIdx, null, pageSize);
  }
}
