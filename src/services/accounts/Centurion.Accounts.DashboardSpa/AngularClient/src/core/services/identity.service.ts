import {Injectable} from "@angular/core";
import {TokenService} from "./token.service";
import {BehaviorSubject, Observable} from "rxjs";
import {distinctUntilChanged} from "rxjs/operators";
import {ClaimNames} from "../models/claim-names.model";
import {Identity} from "./identity.model";
import fastDeepEqual from "fast-deep-equal";


const getValues = (token: any, key: string): any[] => {
  const values = token[key] || [];
  return typeof values === "string"
    ? [values]
    : values;
};

export interface PermissionsSelector {
  any?: string[];
  all?: string[];
}

@Injectable({
  providedIn: "root"
})
export class IdentityService {
  private readonly user$: BehaviorSubject<Identity | null>;

  readonly currentUser$: Observable<Identity | null>;

  constructor(private readonly tokenService: TokenService) {
    this.user$ = new BehaviorSubject<Identity | null>(null);
    this.currentUser$ = this.user$
      .asObservable()
      .pipe(
        distinctUntilChanged((left, right) => !!left && !!right && fastDeepEqual(left, right))
      );

    tokenService.encodedAccessToken$
      .pipe(distinctUntilChanged())
      .subscribe(() => {
        const tokenData = tokenService.decodedAccessToken;
        if (!tokenData) {
          this.user$.next(null);
          return;
        }

        const user: Identity = {
          email: tokenData[ClaimNames.email],
          id: +tokenData[ClaimNames.id],
          avatar: tokenData[ClaimNames.avatar],
          name: tokenData[ClaimNames.name],
          discordId: +tokenData[ClaimNames.discordId],
          discriminator: tokenData[ClaimNames.discriminator],
          roleNames: getValues(tokenData, ClaimNames.roleName),
          roleIds: getValues(tokenData, ClaimNames.roleId).map(r => +r),
          permissions: getValues(tokenData, ClaimNames.permission),
          dashboard: tokenData[ClaimNames.currentDashboardDomain],
          isCurrentDashboardOwner: tokenData[ClaimNames.currentDashboardId] === tokenData[ClaimNames.ownDashboardId]
        };

        this.user$.next(user);
      });
  }

  get currentUser(): Identity | null {
    return this.user$.getValue();
  }

  userHasPermissions(permissions: string | PermissionsSelector): boolean {
    if (!permissions) {
      throw new Error("Null or empty permissions provided for check");
    }

    let allowed: boolean | null = null;
    if (typeof permissions === "string") {
      allowed = this.checkHasPermissions(permissions);
    } else {
      if (permissions.all && permissions.any) {
        throw new Error("Only single value must be specified at a time");
      }

      if (permissions.all) {
        allowed = this.checkHasPermissions(permissions.all, true);
      } else {
        allowed = permissions.any.length === 0 || this.checkHasPermissions(permissions.any);
      }
    }

    return allowed;
  }

  private checkHasPermissions(permissions: string | string[], all = false): boolean {
    if (typeof permissions === "string") {
      return this.checkHasPermissions([permissions]);
    } else {
      const predicateFn = (all ? permissions.every : permissions.some).bind(permissions);
      return predicateFn(p => this.currentUser.permissions.indexOf(p) !== -1);
    }
  }
}
