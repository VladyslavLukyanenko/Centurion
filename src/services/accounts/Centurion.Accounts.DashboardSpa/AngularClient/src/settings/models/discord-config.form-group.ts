import {FormControl, FormGroup, Validators} from "@angular/forms";
import {DiscordOAuthConfigFormGroup} from "./discord-oauth-config.form-group";

export class DiscordConfigFormGroup extends FormGroup {
  constructor() {
    super({
      guildId: new FormControl(null, [Validators.required]),
      roleId: new FormControl(null, [Validators.required]),
      accessToken: new FormControl(null, [Validators.required]),
      botAccessToken: new FormControl(null, [Validators.required]),
      oAuthConfig: new DiscordOAuthConfigFormGroup(),
    });
  }

  get guildIdCtrl(): FormControl {
    return this.get("guildId") as FormControl;
  }

  get roleIdCtrl(): FormControl {
    return this.get("roleId") as FormControl;
  }

  get accessTokenCtrl(): FormControl {
    return this.get("accessToken") as FormControl;
  }

  get botAccessTokenCtrl(): FormControl {
    return this.get("botAccessToken") as FormControl;
  }

  get oAuthConfigCtrl(): DiscordOAuthConfigFormGroup {
    return this.get("oAuthConfig") as DiscordOAuthConfigFormGroup;
  }
}
