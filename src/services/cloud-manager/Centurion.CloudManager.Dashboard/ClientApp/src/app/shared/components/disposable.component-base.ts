import {Component, Directive, OnDestroy} from "@angular/core";
import {MonoTypeOperatorFunction, Observable, OperatorFunction, Subject} from "rxjs";
import {takeUntil} from "rxjs/operators";
import {AsyncProgressTracker} from "../model/async-progress-tracker.model";

@Directive()
export abstract class DisposableComponentBase implements OnDestroy {
  private destroy$: Subject<void> = new Subject<void>();
  readonly asyncTracker = new AsyncProgressTracker();

  get isLoading$(): Observable<boolean> {
    return this.asyncTracker.isLoading$;
  }

  dispose(): void {
    this.destroy$.next();
    this.destroy$.complete();
    this.asyncTracker.dispose();
  }

  protected untilDestroy<T>(): MonoTypeOperatorFunction<T> {
    return takeUntil(this.destroy$);
  }

  protected loadingSignal<T>(): OperatorFunction<T, T> {
    return this.asyncTracker.loadingSignal<T>();
  }

  protected async executeAsync<T>(callable: Promise<T>): Promise<T> {
    return this.asyncTracker.executeAsync(callable);
  }

  protected async executeAsAsync<T>(callable: Observable<T>): Promise<T> {
    return this.asyncTracker.executeAsAsync(callable);
  }

  ngOnDestroy(): void {
    this.dispose();
  }
}
