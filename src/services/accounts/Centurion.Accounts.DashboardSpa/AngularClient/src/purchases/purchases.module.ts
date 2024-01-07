import {NgModule} from "@angular/core";
import {SharedModule} from "../shared/shared.module";
import {PurchasesRoutingModule} from "./purchases-routing.module";
import {PurchasesPageComponent} from "./components/purchases-page/purchases-page.component";
import {UnbindLicenseKeyDialogComponent} from "./components/unbind-license-key-dialog/unbind-license-key-dialog.component";


@NgModule({
  declarations: [PurchasesPageComponent, UnbindLicenseKeyDialogComponent],
  imports: [
    SharedModule,
    PurchasesRoutingModule
  ]
})
export class PurchasesModule {
}
