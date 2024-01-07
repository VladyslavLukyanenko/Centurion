import {ChangeDetectionStrategy, Component, Input, Output, EventEmitter} from '@angular/core';
import {StaffMemberData} from "../../../dashboards-api";
import {IdentityService} from "../../../core/services/identity.service";

@Component({
  selector: 'app-staff-member-card',
  templateUrl: './staff-member-card.component.html',
  styleUrls: ['./staff-member-card.component.less'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class StaffMemberCardComponent {
  @Input() member: StaffMemberData;
  @Output() removeClick = new EventEmitter<Event>();

  constructor(private identityService: IdentityService) {
  }

  get isCurrentUser(): boolean {
    return this.identityService.currentUser.id === this.member.id;
  }
}
