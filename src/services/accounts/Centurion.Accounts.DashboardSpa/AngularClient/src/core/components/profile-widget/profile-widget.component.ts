import {ChangeDetectionStrategy, Component} from "@angular/core";
import {IdentityService} from "../../services/identity.service";
import {DisposableComponentBase} from "../../../shared/components/disposable.component-base";

@Component({
  selector: "app-profile-widget",
  templateUrl: "./profile-widget.component.html",
  styleUrls: ["./profile-widget.component.scss"],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ProfileWidgetComponent extends DisposableComponentBase {
  user = this.identityService.currentUser$;
  constructor(readonly identityService: IdentityService) {
    super();
  }
}
