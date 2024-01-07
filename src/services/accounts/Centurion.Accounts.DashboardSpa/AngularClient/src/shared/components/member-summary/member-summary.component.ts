import {ChangeDetectionStrategy, Component, Input} from '@angular/core';
import {BoundMemberRoleData, DiscordRoleInfo, MemberSummaryData} from "../../../dashboards-api";
import * as moment from "moment";

function shadeColor(color: string, percent: number) {
  const R = parseInt(color.substring(1, 3), 16);
  const G = parseInt(color.substring(3, 5), 16);
  const B = parseInt(color.substring(5, 7), 16);

  return `rgba(${R}, ${G}, ${B}, ${1 - percent})`
}

const format = "DD/MM/yyyy";

const defaultColor = "#494949";
const lightenFactor = .92;

@Component({
  selector: 'app-member-summary',
  templateUrl: './member-summary.component.html',
  styleUrls: ['./member-summary.component.less'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class MemberSummaryComponent {
  @Input() summary: MemberSummaryData;

  get defaultColor(): string {
    return defaultColor;
  }

  getBgColor(r: any): string {
    if (!r["__bgCache__"]) {
      r["__bgCache__"] = shadeColor(r.colorHex || defaultColor, lightenFactor);
    }

    return r["__bgCache__"];
  }

  get formattedJoinedDate(): string {
    return moment(this.summary?.joinedAt).format(format);
  }

  trackById = (_: number, r: DiscordRoleInfo) => r.id;
}
