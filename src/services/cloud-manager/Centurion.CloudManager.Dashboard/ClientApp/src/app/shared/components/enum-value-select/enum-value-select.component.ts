import {ChangeDetectionStrategy, Component, forwardRef, Injector, Input, Type} from "@angular/core";
import {ControlValueAccessorBase} from "../control-value-accessor-base";
import {KeyValuePair} from "../../../core/models/key-value-pair.model";
import {NG_VALUE_ACCESSOR} from "@angular/forms";

@Component({
  selector: "r-enum-value-select",
  templateUrl: "./enum-value-select.component.html",
  styleUrls: ["./enum-value-select.component.scss"],
  changeDetection: ChangeDetectionStrategy.OnPush,
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => EnumValueSelectComponent),
      multi: true
    }
  ]
})
export class EnumValueSelectComponent extends ControlValueAccessorBase {
  @Input()
  enumType: Type<any>;

  @Input()
  placeholder: string;

  @Input()
  namePrefix: string;

  @Input()
  size: "large" | "default" | "small";

  constructor(injector: Injector) {
    super(injector);
  }

  getKeyValuePairs(): KeyValuePair<any>[] {
    return Object.keys(this.enumType)
      .filter(k => isNaN(+this.enumType[k]))
      .map(k => ({
        key: k,
        value: this.enumType[k]
      }));
  }

  getFriendlyName(key: string) {
    return this.namePrefix
      ? this.namePrefix + key
      : key;
  }
}
