import {Injectable} from "@angular/core";
import {Observable, Subscriber} from "rxjs";

import {TokenService} from "./token.service";
import {ActivatedRoute, Router} from "@angular/router";
import {AppSettingsService} from "./app-settings.service";
import {HttpClient} from "@angular/common/http";
import {map} from "rxjs/operators";
import {AuthenticationApiClient} from "./authentication.api-client";
import {ExternalAuthToken} from "../models/external-auth-token.model";

@Injectable({
  providedIn: "root"
})
export class AuthenticationService {
  private pendingReauthentications = new Array<Subscriber<any>>();
  private isReauthenticating = false;

  constructor(
    private readonly tokenService: TokenService,
    private readonly settings: AppSettingsService,
    private readonly _router: Router,
    private readonly _activatedRoute: ActivatedRoute,
    private http: HttpClient,
    private authClient: AuthenticationApiClient,
  ) {
  }


  logOut(): void {
    this.tokenService.removeSecurityToken();
  }

  isAuthenticated(): boolean {
    return this.tokenService.isAccessTokenValid;
  }

  canReauthenticate(): boolean {
    return this.tokenService.isRefreshTokenValid;
  }

  authenticateWithExternal(request: ExternalAuthToken): Observable<boolean> {
    return this.authClient.authenticateWithExternal(request)
      .pipe(
        map(token => {
          if (token) {
            this.tokenService.setSecurityToken(token);
          }

          return !!token;
        })
      );
  }

  reauthenticate(): Observable<boolean> {
    return new Observable<boolean>(observer => {
      this.pendingReauthentications.push(observer);
      if (!this.isReauthenticating) {
        this.isReauthenticating = true;
        this.authClient.reauthenticate(this.tokenService.refreshToken, this.tokenService.encodedAccessToken)
          .subscribe(token => {
            this.isReauthenticating = false;
            const hasToken = !!token;
            if (hasToken) {
              this.tokenService.setSecurityToken(token);
            }

            while (this.pendingReauthentications.length) {
              const pr = this.pendingReauthentications.pop();
              pr.next(hasToken);
              pr.complete();
            }
          }, err => {
            this.isReauthenticating = false;
            while (this.pendingReauthentications.length) {
              const pr = this.pendingReauthentications.pop();
              pr.error(err);
              pr.complete();
            }

            this.logOut();
          });
      }
    });
  }
}
