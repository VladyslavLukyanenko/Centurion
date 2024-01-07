import {
  Component,
  OnInit,
  ChangeDetectionStrategy,
  Input,
  Output,
  EventEmitter,
  ChangeDetectorRef
} from "@angular/core";
import {MemberRoleData, MemberRolesService, PermissionInfoData, PermissionsService} from "../../../dashboards-api";
import {BehaviorSubject} from "rxjs";
import {DisposableComponentBase} from "../../../shared/components/disposable.component-base";
import {FormControl} from "@angular/forms";
import {ConfirmationService} from "primeng/api";

@Component({
  selector: "app-roles-edit-dialog",
  templateUrl: "./roles-edit-dialog.component.html",
  styleUrls: ["./roles-edit-dialog.component.less"],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class RolesEditDialogComponent extends DisposableComponentBase implements OnInit {
  @Input() isVisible = false;
  @Output() isVisibleChange = new EventEmitter<boolean>();
  @Output() roleCreated = new EventEmitter<string>();
  @Output() roleRemoved = new EventEmitter<number>();
  selectEditRole: MemberRoleData = null;
  roles$ = new BehaviorSubject<MemberRoleData[]>([]);
  supportedPermissions$ = new BehaviorSubject<PermissionInfoData[]>([]);

  newRoleNameCtrl = new FormControl();

  constructor(private memberRolesService: MemberRolesService,
              private permissionsService: PermissionsService,
              private confirmService: ConfirmationService,
              private cd: ChangeDetectorRef) {
    super();
  }

  async ngOnInit() {
    await this.refreshData();
    const permissions = await this.asyncTracker.executeAsAsync(this.permissionsService.permissionsGetUsedPermissions());
    this.supportedPermissions$.next(permissions.payload);
  }

  private async refreshData() {
    const r = await this.asyncTracker.executeAsAsync(
      this.memberRolesService.memberRolesGetRoles()
    );

    this.roles$.next(r.payload);
  }

  selectRoleToEdit(r: MemberRoleData) {
    this.selectEditRole = r;
  }

  toggleRolePermission(r: MemberRoleData, permission: string) {
    const idx = r.permissions.indexOf(permission);
    if (idx === -1) {
      r.permissions.push(permission);
    } else {
      r.permissions.splice(idx, 1);
    }
  }

  async deleteRole(r: MemberRoleData, e: Event) {
    this.confirmService.confirm({
      message: `Are you sure to remove role ${r.name}`,
      target: e.target,
      icon: "pi pi-exclamation-triangle",
      accept: async () => {
        await this.asyncTracker.executeAsAsync(this.memberRolesService.memberRolesRemoveRole(r.id));
        await this.refreshData();
        this.selectEditRole = null;
        this.cd.detectChanges();
        this.roleRemoved.emit(r.id);
      }
    });
  }

  async createRole() {
    const name = this.newRoleNameCtrl.value;
    this.newRoleNameCtrl.setValue(null);

    await this.asyncTracker.executeAsAsync(
      this.memberRolesService.memberRolesCreate({
        name
      })
    );

    await this.refreshData();
    this.roleCreated.emit(name);
  }

  async updateRole(role: MemberRoleData) {
    this.selectEditRole = null;

    await this.asyncTracker.executeAsAsync(
      this.memberRolesService.memberRolesUpdate(role.id, role)
    );

    await this.refreshData();
  }
}
