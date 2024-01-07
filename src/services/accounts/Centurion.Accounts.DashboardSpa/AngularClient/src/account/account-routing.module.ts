import {RouterModule, Routes} from "@angular/router";
import {LoginComponent} from "./components/login/login.component";
import {NgModule} from "@angular/core";
import {DiscordCallbackComponent} from "./components/discord-callback/discord-callback.component";
import {NotFoundComponent} from "../core/components/not-found/not-found.component";

const routes: Routes = [
  {
    path: "",
    component: LoginComponent,
    pathMatch: "full"
  },
  {
    path: "oauth2/discord/callback",
    component: DiscordCallbackComponent
  },
  {
    path: "**",
    component: NotFoundComponent
  }
];

@NgModule({
  imports: [
    RouterModule.forChild(routes)
  ],
  exports: [
    RouterModule
  ]
})
export class AccountRoutingModule {

}
