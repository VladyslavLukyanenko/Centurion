import {AppSettingsService} from "./core/services/app-settings.service";
import {ChangeDetectionStrategy, Component, OnDestroy, OnInit} from "@angular/core";
import {ActivatedRoute, Router} from "@angular/router";
import {TranslateService} from "@ngx-translate/core";
import {IdentityService} from "./core/services/identity.service";
import {DisposableComponentBase} from "./shared/components/disposable.component-base";
import {BehaviorSubject, combineLatest, Observable} from "rxjs";
import {distinctUntilChanged, map, shareReplay} from "rxjs/operators";
import {environment} from "../environments/environment";

@Component({
  selector: "r-host",
  templateUrl: "./host.component.html",
  styleUrls: ["./host.component.scss"],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class HostComponent extends DisposableComponentBase implements OnInit, OnDestroy {
  private _initialized$ = new BehaviorSubject<boolean>(false);
  initialized$: Observable<boolean>;

  constructor(
    private readonly activatedRoute: ActivatedRoute,
    private readonly userService: IdentityService,
    private settings: AppSettingsService,
    private router: Router,
    private readonly translateService: TranslateService) {
    super();
    this.translateService.setDefaultLang(environment.defaultCultureCode);
  }

  ngOnInit() {
    const isInitialized$ = this._initialized$.asObservable()
      .pipe(
        distinctUntilChanged(),
        shareReplay()
      );

    this.initialized$ = combineLatest([isInitialized$, this.userService.currentUser$])
      .pipe(
        map(([isInitialized, currUser]) => isInitialized || !currUser),
        shareReplay()
      );

    combineLatest([this.activatedRoute.paramMap])
      .pipe(this.untilDestroy())
      .subscribe(async ([params]) => {
        this._initialized$.next(true);
      });
  }

  ngOnDestroy(): void {
    super.ngOnDestroy();
  }
}
