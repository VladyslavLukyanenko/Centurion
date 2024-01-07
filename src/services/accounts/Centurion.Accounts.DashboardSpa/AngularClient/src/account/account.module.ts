import {NgModule} from "@angular/core";
import {LoginComponent} from "./components/login/login.component";
import {DiscordCallbackComponent} from "./components/discord-callback/discord-callback.component";
import {SharedModule} from "../shared/shared.module";
import {AccountRoutingModule} from "./account-routing.module";


@NgModule({
  declarations: [LoginComponent, DiscordCallbackComponent],
  imports: [
    SharedModule,
    AccountRoutingModule
  ]
})
export class AccountModule {
}
