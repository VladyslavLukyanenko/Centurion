export interface PagedList<T> {
  readonly isFirst?: boolean;
  readonly isLast?: boolean;
  readonly pageIndex?: number;
  readonly limit?: number;
  readonly count?: number;
  readonly totalElements?: number;
  readonly totalPages?: number;
  readonly isEmpty?: boolean;
  readonly content?: Array<T>;
}
