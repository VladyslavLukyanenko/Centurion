import {ChangeDetectionStrategy, Component, OnInit} from '@angular/core';
import {ActivatedRoute, Router} from "@angular/router";
import {DisposableComponentBase} from "../../../shared/components/disposable.component-base";
import {environment} from "../../../environments/environment";

@Component({
  selector: 'app-discord-callback',
  templateUrl: './discord-callback.component.html',
  styleUrls: ['./discord-callback.component.less'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class DiscordCallbackComponent extends DisposableComponentBase implements OnInit {

  constructor(private router: Router,
              private activatedRoute: ActivatedRoute) {
    super();
  }

  ngOnInit(): void {
    this.activatedRoute.queryParamMap
      .pipe(this.untilDestroy())
      .subscribe(p => {
        localStorage.setItem(environment.auth.discord.codeKey, p.get("code")!);
      });
  }
}
