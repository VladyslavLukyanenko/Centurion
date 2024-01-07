import {Injectable} from "@angular/core";
import {AnalyticsService} from "../../dashboards-api";

const sessionIdStoreKey = "gg.centurion.dashboard.session.id";

@Injectable({
  providedIn: "root"
})
export class UserSessionService {
  constructor(private analyticsService: AnalyticsService) {
  }

  async refreshSession(): Promise<void> {
    const oldId = localStorage.getItem(sessionIdStoreKey);
    const r = await this.analyticsService.analyticsRefreshOrCreateSession(oldId).toPromise();
    localStorage.setItem(sessionIdStoreKey, r.payload);
  }
}
