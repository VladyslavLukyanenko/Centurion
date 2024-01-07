import {ChangeDetectionStrategy, Component, EventEmitter, Input, OnInit, Output} from "@angular/core";
import {KeyValuePair} from "../../../core/models/key-value-pair.model";
import {MemberRoleAssignmentData, MemberRoleBindingsService, StaffMemberData} from "../../../dashboards-api";
import {BehaviorSubject, combineLatest} from "rxjs";
import {FormControl} from "@angular/forms";
import {DisposableComponentBase} from "../../../shared/components/disposable.component-base";
import {debounceTime, map} from "rxjs/operators";
import {MembersDataSource} from "../../../core/data-sources/members.data-source";
import {Observable} from "rxjs/internal/Observable";


interface StaffMemberCandidate extends StaffMemberData {
  selectedRole?: number;
}

@Component({
  selector: "app-staff-edit-dialog",
  templateUrl: "./staff-edit-dialog.component.html",
  styleUrls: ["./staff-edit-dialog.component.less"],
  providers: [MembersDataSource],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class StaffEditDialogComponent extends DisposableComponentBase implements OnInit {
  @Input() isVisible = false;
  @Input() roles: KeyValuePair<string, number>[] = [];
  @Output() isVisibleChange = new EventEmitter<boolean>();
  @Output() rolesAssigned = new EventEmitter<MemberRoleAssignmentData[]>();

  searchCtrl = new FormControl();

  assignedMembers$ = new BehaviorSubject<StaffMemberCandidate[]>([]);
  selectedMemberIds$: Observable<number[]>;

  constructor(private membersService: MemberRoleBindingsService, readonly membersDataSource: MembersDataSource) {
    super();
    membersDataSource.setIncludeStaff(false);
  }

  async ngOnInit(): Promise<void> {
    this.searchCtrl.valueChanges
      .pipe(
        this.untilDestroy(),
      )
      .subscribe(async s => this.membersDataSource.setSearchTerm(s));

    this.selectedMemberIds$ = this.assignedMembers$.pipe(map(_ => _.map(m => m.id)));
  }

  toggleAssigningMember(member: StaffMemberCandidate, role?: number): void {
    if (role) {
      this.assignedMembers$.next([...this.assignedMembers$.value, member]);
      member.selectedRole = role;
    } else {
      member.selectedRole = null;
      this.assignedMembers$.next(this.assignedMembers$.value.filter(m => m !== member));
    }
  }

  async assignRoles(): Promise<void> {
    if (!this.assignedMembers$.value.length) {
      return;
    }

    const payload: MemberRoleAssignmentData[] = this.assignedMembers$.value.map(m => ({
      memberRoleId: m.selectedRole,
      userId: m.id
    }));

    this.assignedMembers$.next([]);
    try {
      await this.asyncTracker.executeAsAsync(this.membersService.memberRoleBindingsAssignRoles(payload));
    } catch {

    }

    await this.membersDataSource.refreshData();
    this.rolesAssigned.emit(payload);
  }
}
