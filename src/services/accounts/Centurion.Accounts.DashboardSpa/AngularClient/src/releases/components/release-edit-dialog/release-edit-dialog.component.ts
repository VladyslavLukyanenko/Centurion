import {ChangeDetectionStrategy, Component, EventEmitter, Input, OnInit, Output} from "@angular/core";
import {ReleaseFormGroup} from "../../models/release.form-group";
import {KeyValuePair} from "../../../core/models/key-value-pair.model";
import {PlanData, PlansService, ReleasesService, ReleaseType} from "../../../dashboards-api";
import {map} from "rxjs/operators";
import {FormUtil} from "../../../core/services/form.util";
import {DisposableComponentBase} from "../../../shared/components/disposable.component-base";
import {BehaviorSubject} from "rxjs";


@Component({
  selector: "app-release-edit-dialog",
  templateUrl: "./release-edit-dialog.component.html",
  styleUrls: ["./release-edit-dialog.component.less"],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ReleaseEditDialogComponent extends DisposableComponentBase implements OnInit {
  @Input() isVisible = false;
  @Output() isVisibleChange = new EventEmitter<boolean>();
  @Output() saved = new EventEmitter<void>();

  keyTypes = [
    "Renewal",
    "Lifetime"
  ];

  releaseTypes: KeyValuePair<string, ReleaseType>[] = [
    {key: ReleaseType.Fcfs, value: ReleaseType.Fcfs}
  ];

  activeStates: KeyValuePair<string, boolean>[] = [
    {key: "Deactivated", value: false},
    {key: "Active", value: true}
  ];

  form = new ReleaseFormGroup();
  plans$ = new BehaviorSubject<PlanData[]>([]);

  constructor(private plansService: PlansService,
              private releasesService: ReleasesService) {
    super();
  }

  async create(): Promise<void> {
    if (this.form.invalid) {
      FormUtil.validateAllFormFields(this.form);
      return;
    }

    await this.asyncTracker.executeAsAsync(
      this.releasesService.releasesCreate(this.form.value)
    );

    this.saved.emit();
  }

  async ngOnInit(): Promise<void> {
    const data = await this.asyncTracker.executeAsAsync(
      this.plansService.plansGetAll()
        .pipe(
          map(_ => _.payload)
        )
    );

    this.plans$.next(data);
  }
}
