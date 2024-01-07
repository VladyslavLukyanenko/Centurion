import {Directive, HostBinding, OnDestroy} from "@angular/core";
import {Disposable} from "../models/disposable.model";

@Directive()
// tslint:disable-next-line:directive-class-suffix
export abstract class DisposableComponentBase extends Disposable implements OnDestroy {
  ngOnDestroy(): void {
    this.dispose();
  }
}
