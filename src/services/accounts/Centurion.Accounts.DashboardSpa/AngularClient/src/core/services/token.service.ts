import {Injectable} from "@angular/core";
import {JwtHelperService} from "@auth0/angular-jwt";
import {BehaviorSubject} from "rxjs";
import {ApplicationSecurityToken} from "../models/application-security-token.model";

const ACCESS_TOKEN_STORE_KEY = "space.dashboards.token.access";
const REFRESH_TOKEN_EXPIRY_STORE_KEY = "space.dashboards.token.refresh.expiry";
const REFRESH_TOKEN_LIFETIME = 604789988; // 7 days

@Injectable({
  providedIn: "root"
})
export class TokenService {
  private jwtService = new JwtHelperService();
  private accessToken$ = new BehaviorSubject<string | null>(this.encodedAccessToken);
  private refreshToken$ = new BehaviorSubject<string | null>(this.refreshToken);
  readonly encodedAccessToken$ = this.accessToken$.asObservable();
  readonly encodedRefreshToken$ = this.refreshToken$.asObservable();

  get encodedAccessToken(): string | null {
    return localStorage.getItem(ACCESS_TOKEN_STORE_KEY);
  }

  get refreshToken(): string {
    const token = this.decodedAccessToken;
    return token && token.discord_refresh_token || null;
  }

  get decodedAccessToken(): any {
    const token = this.encodedAccessToken;
    if (!token) {
      return null;
    }

    return this.jwtService.decodeToken(token);
  }

  get accessTokenExpirationDate(): Date | null {
    const token = this.encodedAccessToken;
    if (!token) {
      return null;
    }

    return this.jwtService.getTokenExpirationDate(token);
  }

  get refreshTokenExpirationDate(): Date {
    const expiry = localStorage.getItem(REFRESH_TOKEN_EXPIRY_STORE_KEY);
    if (!expiry) {
      return new Date(-1);
    }

    return new Date(+expiry);
  }

  get isRefreshTokenValid(): boolean {
    return !!this.refreshToken && this.refreshTokenExpirationDate > new Date();
  }

  get isAccessTokenValid(): boolean {
    return !!this.encodedAccessToken && !this.jwtService.isTokenExpired(this.encodedAccessToken);
  }

  setSecurityToken(token: ApplicationSecurityToken): void {
    this.setEncodedAccessToken(token.access_token);
    const decoded = this.decodedAccessToken;
    this.setEncodedRefreshToken(decoded.discord_refresh_token);
  }

  removeSecurityToken(): void {
    localStorage.removeItem(ACCESS_TOKEN_STORE_KEY);
    localStorage.removeItem(REFRESH_TOKEN_EXPIRY_STORE_KEY);

    this.accessToken$.next(null);
    this.refreshToken$.next(null);
  }

  private setEncodedAccessToken(token: string): void {
    if (!token) {
      throw new Error("Trying to set null value for token");
    }

    localStorage.setItem(ACCESS_TOKEN_STORE_KEY, token);
    this.accessToken$.next(token);
  }

  private setEncodedRefreshToken(token: string): void {
    if (!token) {
      throw new Error("Trying to set null value for token");
    }

    localStorage.setItem(REFRESH_TOKEN_EXPIRY_STORE_KEY, (Date.now() + REFRESH_TOKEN_LIFETIME).toString());
    this.refreshToken$.next(token);
  }
}
