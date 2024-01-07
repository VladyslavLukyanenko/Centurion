import {Component, OnInit, ChangeDetectionStrategy, Input, Output, EventEmitter} from '@angular/core';
import {DisposableComponentBase} from "../../../shared/components/disposable.component-base";
import {KeyValuePair} from "../../../core/models/key-value-pair.model";
import {ReleaseFormGroup} from "../../../releases/models/release.form-group";
import {BehaviorSubject} from "rxjs";
import {LicenseKeysService, PlanData, PlansService, ReleasesService} from "../../../dashboards-api";
import {FormUtil} from "../../../core/services/form.util";
import {map} from "rxjs/operators";
import {LicensesGenerateFormGroup} from "../../../releases/models/licenses-generate.form-group";

@Component({
  selector: 'app-licenses-create-dialog',
  templateUrl: './licenses-create-dialog.component.html',
  styleUrls: ['./licenses-create-dialog.component.less'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class LicensesCreateDialogComponent extends DisposableComponentBase implements OnInit {
  @Input() isVisible = false;
  @Output() isVisibleChange = new EventEmitter<boolean>();
  @Output() saved = new EventEmitter<void>();

  keyTypes: KeyValuePair<string, boolean>[] = [
    {key: "Renewal", value: false},
    {key: "Lifetime", value: true},
  ];

  form = new LicensesGenerateFormGroup();
  plans$ = new BehaviorSubject<PlanData[]>([]);

  constructor(private plansService: PlansService,
              private licenseKeysService: LicenseKeysService) {
    super();
  }

  async create() {
    if (this.form.invalid) {
      FormUtil.validateAllFormFields(this.form);
      return;
    }

    await this.asyncTracker.executeAsAsync(
      this.licenseKeysService.licenseKeysGenerate(this.form.value)
    );

    this.saved.emit();
  }

  async ngOnInit() {
    const data = await this.asyncTracker.executeAsAsync(
      this.plansService.plansGetAll()
        .pipe(
          map(_ => _.payload)
        )
    );

    this.plans$.next(data);
  }
}
