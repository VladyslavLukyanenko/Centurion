import {MonoTypeOperatorFunction, Observable, OperatorFunction, Subject} from "rxjs";
import {takeUntil, last} from "rxjs/operators";
import {AsyncProgressTracker} from "./async-progress-tracker.model";

export abstract class Disposable {
  private destroy$: Subject<void> = new Subject<void>();
  readonly asyncTracker = new AsyncProgressTracker();
  readonly isLoading$ =  this.asyncTracker.isLoading$;

  dispose(): void {
    this.destroy$.next();
    this.destroy$.complete();
    this.asyncTracker.dispose();
  }

  protected untilDestroy<T>(): MonoTypeOperatorFunction<T> {
    return takeUntil(this.destroy$);
  }
}
