import {Injectable} from "@angular/core";
import {JwtHelperService} from "@auth0/angular-jwt";
import {BehaviorSubject, Observable} from "rxjs";
import {environment} from "../../../environments/environment";
import {ApplicationSecurityToken} from "../models/application-security-token.model";

const ACCESS_TOKEN_STORE_KEY = "monitors-panel.token.access";
const REFRESH_TOKEN_STORE_KEY = "monitors-panel.token.refresh";
const SESSION_EXP_TIME_STORE_KEY = "monitors-panel.token.access.expiration";
const REFRESH_EXP_TIME_STORE_KEY = "monitors-panel.token.refresh.expiration";

@Injectable({
  providedIn: "root"
})
export class TokenService {
  private readonly _jwtService: JwtHelperService = new JwtHelperService();
  private _accessTokenSubject$ = new BehaviorSubject<string>(this.encodedAccessToken);
  private _refreshTokenSubject$ = new BehaviorSubject<string>(this.refreshToken);

  get encodedAccessToken$(): Observable<string> {
    return this._accessTokenSubject$.asObservable();
  }

  get encodedRefreshToken$(): Observable<string> {
    return this._refreshTokenSubject$.asObservable();
  }

  get encodedAccessToken(): string {
    return localStorage.getItem(ACCESS_TOKEN_STORE_KEY);
  }

  get refreshToken(): string {
    return localStorage.getItem(REFRESH_TOKEN_STORE_KEY);
  }

  get decodedAccessToken(): any {
    return this._jwtService.decodeToken(this.encodedAccessToken);
  }

  get accessTokenExpirationDate(): Date {
    return this._jwtService.getTokenExpirationDate(this.encodedAccessToken);
  }

  get refreshTokenExpirationDate(): Date {
    if (!(REFRESH_EXP_TIME_STORE_KEY in localStorage)) {
      return new Date(0);
    }

    return new Date(+localStorage.getItem(REFRESH_EXP_TIME_STORE_KEY));
  }

  get isRefreshTokenValid(): boolean {
    const sessionExpTime = localStorage.getItem(SESSION_EXP_TIME_STORE_KEY);
    if (sessionExpTime && new Date().getTime() > new Date(sessionExpTime).getTime()) {
      return false;
    }
    return !!this.refreshToken && this.refreshTokenExpirationDate > new Date();
  }

  get isAccessTokenValid(): boolean {
    return !!this.encodedAccessToken && !this._jwtService.isTokenExpired(this.encodedAccessToken);
  }

  setSecurityToken(token: ApplicationSecurityToken): void {
    if (localStorage.getItem(SESSION_EXP_TIME_STORE_KEY)) {
      this.setTokenExpirationTime(token.access_token);
    }
    this.setEncodedAccessToken(token.access_token);
    this.setEncodedRefreshToken(token.refresh_token);
  }

  removeSecurityToken() {
    localStorage.removeItem(SESSION_EXP_TIME_STORE_KEY);
    localStorage.removeItem(REFRESH_EXP_TIME_STORE_KEY);

    localStorage.removeItem(ACCESS_TOKEN_STORE_KEY);
    this._accessTokenSubject$.next(null);
    localStorage.removeItem(REFRESH_TOKEN_STORE_KEY);
    this._refreshTokenSubject$.next(null);
  }

  private setEncodedAccessToken(token: string) {
    if (!token) {
      throw new Error("Trying to set null value for token");
    }
    localStorage.setItem(ACCESS_TOKEN_STORE_KEY, token);
    this._accessTokenSubject$.next(token);
  }

  private setEncodedRefreshToken(token: string) {
    if (!token) {
      throw new Error("Trying to set null value for token");
    }

    localStorage.setItem(REFRESH_TOKEN_STORE_KEY, token);
    localStorage.setItem(REFRESH_EXP_TIME_STORE_KEY, String(Date.now() + environment.auth.refreshTokenLifetime * 1000));
    this._refreshTokenSubject$.next(token);
  }

  private setTokenExpirationTime(token: string) {
    const expTime: string = (this._jwtService.decodeToken(token).exp).toString();
    localStorage.setItem(SESSION_EXP_TIME_STORE_KEY, expTime);
  }
}
