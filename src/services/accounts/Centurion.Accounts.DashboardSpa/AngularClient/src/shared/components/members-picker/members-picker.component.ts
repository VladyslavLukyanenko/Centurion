import {Component, OnInit, ChangeDetectionStrategy, Input, TemplateRef, Output, EventEmitter} from "@angular/core";
import {MembersDataSource} from "../../../core/data-sources/members.data-source";
import {DisposableComponentBase} from "../disposable.component-base";
import {FormControl} from "@angular/forms";
import {StaffMemberData} from "../../../dashboards-api";

@Component({
  selector: "app-members-picker",
  templateUrl: "./members-picker.component.html",
  styleUrls: ["./members-picker.component.less"],
  providers: [MembersDataSource],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class MembersPickerComponent extends DisposableComponentBase implements OnInit {
  @Input() dataSource: MembersDataSource;
  @Input() actionsTmpl: TemplateRef<StaffMemberData>;
  @Input() selectedMemberIds: number[] = [];
  @Input() listHeight = 235;

  @Output() memberClick = new EventEmitter<StaffMemberData>();
  searchCtrl = new FormControl();

  constructor() {
    super();
  }

  trackById = (ix: number, m: StaffMemberData) => m.id;

  ngOnInit(): void {
    this.searchCtrl.valueChanges
      .pipe(
        this.untilDestroy(),
      )
      .subscribe(async s => this.dataSource.setSearchTerm(s));
  }

  isSelected(member: StaffMemberData): boolean {
    const isSelected = this.selectedMemberIds.indexOf(member.id) > -1;
    console.log("Is selected - " + isSelected);
    return isSelected;
  }
}
