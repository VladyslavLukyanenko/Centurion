import {Directive, Input, TemplateRef, ViewContainerRef} from "@angular/core";
import {IdentityService, PermissionsSelector} from "../../core/services/identity.service";


@Directive({
  selector: "[appPermission]"
})
export class PermissionDirective {
  private hasView = false;

  constructor(
    private templateRef: TemplateRef<any>,
    private viewContainer: ViewContainerRef,
    private id: IdentityService
  ) {
  }

  @Input() set appPermission(arg: null | PermissionsSelector | string) {
    const allowed = !arg || this.id.userHasPermissions(arg);
    if (this.hasView === allowed) {
      return;
    }

    this.hasView = allowed;
    if (allowed) {
      this.viewContainer.createEmbeddedView(this.templateRef);
    } else {
      this.viewContainer.clear();
    }
  }
}
