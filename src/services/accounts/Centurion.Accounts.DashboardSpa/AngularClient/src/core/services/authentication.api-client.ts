import {Injectable} from "@angular/core";
import {ApplicationSecurityToken} from "../models/application-security-token.model";
import {HttpClient} from "@angular/common/http";
import {Observable} from "rxjs/internal/Observable";
import {environment} from "../../environments/environment";
import {ExternalAuthToken} from "../models/external-auth-token.model";

const authConfig = {
  clientId: "discord-auth.client",
  grantType: {
    refresh: "discord-token.refresh",
    auth: "discord-token.auth"
  }
};

@Injectable({
  providedIn: "root"
})
export class AuthenticationApiClient {
  constructor(private http: HttpClient) {
  }

  authenticateWithExternal(request: ExternalAuthToken): Observable<ApplicationSecurityToken> {
    const authData = new URLSearchParams();
    authData.append("code", request.code);
    authData.append("dashboard", location.toString());
    authData.append("grant_type", authConfig.grantType.auth);
    authData.append("client_id", authConfig.clientId);

    return this.http.post<ApplicationSecurityToken>(this.expandUrl("/connect/token"), authData.toString(), {
      headers: {
        "Content-Type": "application/x-www-form-urlencoded"
      }
    });
  }

  reauthenticate(refreshToken: string, accessToken: string): Observable<ApplicationSecurityToken | null> {
    const refreshData = new URLSearchParams();
    refreshData.append("refresh_token", refreshToken);
    refreshData.append("access_token", accessToken);
    refreshData.append("grant_type", authConfig.grantType.refresh);
    refreshData.append("client_id", authConfig.clientId);
    refreshData.append("dashboard", location.toString());

    return this.http.post<ApplicationSecurityToken | null>(this.expandUrl("/connect/token"), refreshData.toString(), {
      headers: {
        "Content-Type": "application/x-www-form-urlencoded"
      }
    });
  }

  private expandUrl(relativeUrl: string): string {
    return environment.apiHostUrl + relativeUrl;
  }
}
