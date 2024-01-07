import {BehaviorSubject, Observable, OperatorFunction} from "rxjs";
import {finalize, tap} from "rxjs/operators";

export class AsyncProgressTracker {
  protected isLoading: BehaviorSubject<boolean> = new BehaviorSubject<boolean>(false);

  get isLoading$(): Observable<boolean> {
    return this.isLoading.asObservable();
  }

  loadingSignal<T>(): OperatorFunction<T, T> {
    return (source: Observable<T>) => {
      return source.pipe(
        tap(() => this.isLoading.next(true)),
        finalize(() => this.isLoading.next(false))
      );
    };
  }

  async executeAsync<T>(callable: Promise<T>): Promise<T> {
    try {
      this.isLoading.next(true);
      return await callable;
    } finally {
      this.isLoading.next(false);
    }
  }
  async executeRangeAsync<T>(callables: Promise<any>[]): Promise<any[]> {
    try {
      this.isLoading.next(true);
      const resp = [];
      for (const c of callables) {
        const r = await c;
        resp.push(r);
      }

      return resp;
    } finally {
      this.isLoading.next(false);
    }
  }

  async executeAsAsync<T>(callable: Observable<T>): Promise<T> {
    return this.executeAsync(callable.toPromise());
  }

  async executeRangeAsAsync<T>(callables: Observable<any>[]): Promise<any[]> {
    try {
      this.isLoading.next(true);
      const resp = [];
      for (const c of callables) {
        const r = await c.toPromise();
        resp.push(r);
      }

      return resp;
    } finally {
      this.isLoading.next(false);
    }
  }

  toggleLoading(isLoading: boolean): void {
    this.isLoading.next(isLoading);
  }

  dispose() {
    this.isLoading.complete();
  }
}
