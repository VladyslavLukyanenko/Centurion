import {
  ChangeDetectionStrategy,
  Component,
  EventEmitter,
  Input,
  OnChanges,
  OnInit,
  Output,
  SimpleChanges
} from "@angular/core";
import {FormControl} from "@angular/forms";
import {DisposableComponentBase} from "../../../shared/components/disposable.component-base";
import {LicenseKeysService} from "../../../dashboards-api";
import {DateUtil} from "../../../core/services/date.util";
import * as moment from "moment";
import {BehaviorSubject} from "rxjs";
import {MessageService} from "primeng/api";

@Component({
  selector: "app-add-extra-days-dialog",
  templateUrl: "./add-extra-days-dialog.component.html",
  styleUrls: ["./add-extra-days-dialog.component.less"],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AddExtraDaysDialogComponent extends DisposableComponentBase implements OnInit, OnChanges {
  @Input() isVisible = false;
  @Input() licenseId: number;
  @Input() expiration: string;
  @Output() isVisibleChange = new EventEmitter<boolean>();
  @Output() expirationProlonged = new EventEmitter<number>();

  addExtraDaysCtrl = new FormControl(30);
  computedExpiration$ = new BehaviorSubject<string>(null);

  constructor(private licenseKeysService: LicenseKeysService,
              private messageService: MessageService) {
    super();
  }

  ngOnInit(): void {
    this.addExtraDaysCtrl.valueChanges
      .pipe(
        this.untilDestroy()
      )
      .subscribe(days => this.computeExpirationDate(days));

    this.computeExpirationDate(this.addExtraDaysCtrl.value);
  }


  async addExtraDays(): Promise<void> {
    const daysToAdd = this.addExtraDaysCtrl.value;
    await this.asyncTracker.executeAsAsync(
      this.licenseKeysService.licenseKeysProlongLicenseKey(this.licenseId, daysToAdd));

    this.messageService.add({
      key: "notifications",
      severity: "success",
      detail: "Extra days was added successfully",
      life: 1000
    });

    this.expirationProlonged.emit(daysToAdd);
  }

  ngOnChanges(changes: SimpleChanges): void {
    this.addExtraDaysCtrl.setValue(30);
  }

  private computeExpirationDate(days: number | string): void {
    const date = DateUtil.humanizeDate(moment().add(days, "days"));
    this.computedExpiration$.next(date);
  }
}
