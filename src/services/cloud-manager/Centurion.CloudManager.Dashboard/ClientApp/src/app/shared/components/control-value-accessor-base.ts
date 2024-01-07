import {DisposableComponentBase} from "./disposable.component-base";
import {Component, Directive, Injector, OnInit} from "@angular/core";
import {ControlValueAccessor, FormControl, NgControl} from "@angular/forms";
import {distinctUntilChanged, takeWhile} from "rxjs/operators";

const noop = (_?) => {};
type ValueChangesObserverFn = (newValue?: any) => void;

@Directive()
export abstract class ControlValueAccessorBase extends DisposableComponentBase implements OnInit, ControlValueAccessor {
  protected valueChangeObserver = noop;
  protected onTouchedObserver = noop;

  readonly control = new FormControl();
  protected constructor(private injector: Injector) {
    super();
  }

  ngOnInit() {
    const ngControl = this.injector.get(NgControl, <NgControl> null);
    if (ngControl) {
      ngControl.valueAccessor = this;
    }

    this.control.statusChanges
      .pipe(
        this.untilDestroy(),
        takeWhile(() => !this.control.touched)
      )
      .subscribe(() => {
        this.onTouchedObserver();
      });
    this.control.valueChanges
      .pipe(
        this.untilDestroy(),
        distinctUntilChanged()
      )
      .subscribe(v => {
        this.valueChangeObserver(v);
      });
  }

  registerOnChange(fn: ValueChangesObserverFn): void {
    this.valueChangeObserver = fn;
  }

  registerOnTouched(fn: ValueChangesObserverFn): void {
    this.onTouchedObserver = fn;
  }

  setDisabledState(isDisabled: boolean): void {
    isDisabled ? this.control.disable() : this.control.enable();
  }

  writeValue(obj: any): void {
    this.control.patchValue(obj);
  }
}
