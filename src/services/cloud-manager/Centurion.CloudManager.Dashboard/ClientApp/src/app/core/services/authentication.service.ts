import {Injectable, NgZone} from "@angular/core";
import {Observable, of} from "rxjs";

import {catchError, map} from "rxjs/operators";
import {TokenService} from "./token.service";
import {environment} from "../../../environments/environment";
import {ActivatedRoute, Router} from "@angular/router";
import {AppSettingsService} from "./app-settings.service";
import {HttpClient} from "@angular/common/http";
import {AuthenticationRequest} from "../models/authentication-request.model";
import {ApplicationSecurityToken} from "../models/application-security-token.model";

@Injectable({
  providedIn: "root"
})
export class AuthenticationService {
  constructor(
    private readonly token: TokenService,
    private readonly settings: AppSettingsService,
    private readonly _router: Router,
    private readonly _activatedRoute: ActivatedRoute,
    private http: HttpClient,
    private zone: NgZone
  ) {
  }

  authenticate(request: AuthenticationRequest): Observable<boolean> {
    const authData = new URLSearchParams();
    authData.append("username", request.username);
    authData.append("password", request.password);
    authData.append("grant_type", environment.auth.loginGrantType);
    this.appendClientInfo(authData);

    return this.http.post<ApplicationSecurityToken>(this.expandUrl("/connect/token"), authData.toString(), {
      headers: {
        'Content-Type': 'application/x-www-form-urlencoded'
      }
    })
      .pipe(
        map(token => {
          this.token.setSecurityToken(token);
          return !!token;
        })
      );
  }

  // todo: call logout endpoint
  logOut(): any {
    const routerUrl = this._router.parseUrl(this._router.routerState.snapshot.url);

    if (routerUrl.queryParams.returnUrl) {
      routerUrl.queryParams.returnUrl = null;
    }
    this.token.removeSecurityToken();
  }

  isAuthenticated(): boolean {
    return !!this.token.isAccessTokenValid;
  }

  canReauthenticate(): boolean {
    return this.token.isRefreshTokenValid;
  }

  reauthenticate() {
    const refreshData = new URLSearchParams();
    refreshData.append("refresh_token", this.token.refreshToken);
    refreshData.append("grant_type", environment.auth.refreshGrantType);
    this.appendClientInfo(refreshData);

    return this.http.post<ApplicationSecurityToken>(this.expandUrl("/connect/token"), refreshData.toString(), {
      headers: {
        'Content-Type': 'application/x-www-form-urlencoded'
      }
    })
      .pipe(
        catchError(error => {
          console.error(error);
          this.logOut();
          this.zone.run(() => {
            this.token.removeSecurityToken();
            this._router.navigate(["account"], {
              queryParams: {returnUrl: this._router.routerState.snapshot.url},
              relativeTo: this._activatedRoute.root
            });
          });

          return of(null);
        }),
        map((token: ApplicationSecurityToken) => {
          this.token.setSecurityToken(token);
        })
      );
  }

  private appendClientInfo(authData: URLSearchParams) {
    authData.append("client_id", environment.auth.clientId);
    authData.append("client_secret", environment.auth.clientSecret);
  }

  private expandUrl(relativeUrl: string) {
    return environment.authApiHostName + relativeUrl;
  }
}
