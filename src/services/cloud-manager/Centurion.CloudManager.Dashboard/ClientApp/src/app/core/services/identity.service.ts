import {Injectable} from "@angular/core";
import {TokenService} from "./token.service";
import {BehaviorSubject, Observable} from "rxjs";
import {distinctUntilChanged} from "rxjs/operators";
import {environment} from "../../../environments/environment";
import {Identity} from "../models/identity.model";
import {ClaimNames} from "../models/claim-names.model";


@Injectable({
  providedIn: "root"
})
export class IdentityService {
  private readonly userSubj: BehaviorSubject<Identity>;

  constructor(private readonly tokenService: TokenService) {
    this.userSubj = new BehaviorSubject(null);
    tokenService.encodedAccessToken$.subscribe(() => {
      const tokenData = tokenService.decodedAccessToken;

      if (!tokenData) {
        this.userSubj.next(null);
        return;
      }

      const rawRolesValue: string|string[] = tokenData[ClaimNames.role] || [];
      const roles: string[] = typeof rawRolesValue === "string" ? [rawRolesValue] : rawRolesValue;
      const user: Identity = {
        email: tokenData[ClaimNames.email],
        id: tokenData[ClaimNames.id],
        avatar: tokenData[ClaimNames.avatar],
        firstName: tokenData[ClaimNames.firstName],
        lastName: tokenData[ClaimNames.lastName],
        isAdmin: roles.some(role => role === environment.auth.adminRoleName),
        roles
      };

      this.userSubj.next(user);
    });
  }

  get currentUser(): Identity {
    return this.userSubj.getValue();
  }

  get currentUser$(): Observable<Identity> {
    return this.userSubj.asObservable()
      .pipe(distinctUntilChanged((left, right) => left && right && left.id === right.id));
  }
}
