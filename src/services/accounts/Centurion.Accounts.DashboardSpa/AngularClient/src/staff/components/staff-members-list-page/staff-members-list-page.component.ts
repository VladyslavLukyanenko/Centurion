import {ChangeDetectionStrategy, Component, OnInit} from "@angular/core";
import {
  MemberRoleBindingsService,
  StaffMemberData,
  StaffRoleMembersData
} from "../../../dashboards-api";
import {BehaviorSubject, combineLatest} from "rxjs";
import {ConfirmationService, MenuItem} from "primeng/api";
import {DisposableComponentBase} from "../../../shared/components/disposable.component-base";
import {map} from "rxjs/operators";
import {ToolbarService} from "../../../core/services/toolbar.service";
import {AuthorizePermissions} from "../../../core/decorators/permission.decorator";
import {AppPermissions} from "../../../core/models/permissions.model";

@Component({
  selector: "app-staff-members-list-page",
  templateUrl: "./staff-members-list-page.component.html",
  styleUrls: ["./staff-members-list-page.component.less"],
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: {
    class: "Page StaffMembersListPage"
  }
})
@AuthorizePermissions(AppPermissions.StaffManage, AppPermissions.RolesManage)
export class StaffMembersListPageComponent extends DisposableComponentBase implements OnInit {
  list$ = new BehaviorSubject<StaffRoleMembersData[]>([]);
  items: MenuItem[];

  noData$ = combineLatest([this.list$, this.isLoading$])
    .pipe(map(([list, isLoading]) => !list.length && !isLoading));

  roles$ = this.list$.pipe(
    map(r => r.map(it => ({key: it.name, value: it.id})))
  );

  isStaffDialogVisible = false;
  isRolesDialogVisible = false;

  constructor(private membersService: MemberRoleBindingsService,
              private toolbarService: ToolbarService,
              private confirmService: ConfirmationService) {
    super();
    toolbarService.setTitle("Staff Management");
  }

  async ngOnInit(): Promise<any> {
    this.items = [
      {
        label: "Add new staff",
        command: () => this.isStaffDialogVisible = true
      },
      {
        label: "Add new role",
        command: () => this.isRolesDialogVisible = true
      }
    ];

    await this.refreshData();
  }

  async refreshData(): Promise<any> {
    const membersResponse = await this.asyncTracker.executeAsAsync(
      this.membersService.memberRoleBindingsGetRoles()
    );

    this.list$.next(membersResponse.payload);
  }

  removeStaffMember(member: StaffMemberData, role: StaffRoleMembersData, e: Event): void {
    this.confirmService.confirm({
      message: `Are you sure to remove "${member.name}#${member.discriminator} (${role.name})" from staff members?`,
      target: e.target,
      icon: "pi pi-exclamation-triangle",
      blockScroll: true,
      accept: async () => {
        await this.asyncTracker.executeAsAsync(
          this.membersService.memberRoleBindingsRemoveMember(member.id, role.id)
        );

        await this.refreshData();
      }
    });
  }
}
