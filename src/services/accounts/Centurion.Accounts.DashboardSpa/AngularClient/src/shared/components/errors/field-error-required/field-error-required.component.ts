import {Component, OnInit, ChangeDetectionStrategy, Input, HostBinding} from "@angular/core";
import {AbstractControl} from "@angular/forms";
import {DisposableComponentBase} from "../../disposable.component-base";

@Component({
  selector: "app-field-error-required",
  template: `This field is required`,
  styles: [
    `
      :host {
        display: inline-block;
      }
    `
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: {
    class: "AppForm-errorMessage p-error"
  }
})
export class FieldErrorRequiredComponent extends DisposableComponentBase implements OnInit {
  @Input() control: AbstractControl;

  @HostBinding("class.is-visible") isVisible: boolean;

  constructor() {
    super();
  }

  ngOnInit(): void {
    this.control.statusChanges
      .pipe(this.untilDestroy())
      .subscribe(() => {
        this.isVisible = (this.control.touched || this.control.dirty) && this.control.hasError("required");
      });
  }

}
