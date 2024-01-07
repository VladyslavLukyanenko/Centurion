import {NgModule} from "@angular/core";
import {SettingsPageComponent} from "./components/settings-page/settings-page.component";
import {SharedModule} from "../shared/shared.module";
import {SettingsRoutingModule} from "./settings-routing.module";
import {ProductFeatureEditorComponent} from "./components/product-feature-editor/product-feature-editor.component";
import {QuillModule} from "ngx-quill";


@NgModule({
  declarations: [SettingsPageComponent, ProductFeatureEditorComponent],
  imports: [
    SharedModule,
    SettingsRoutingModule,
    QuillModule
  ]
})
export class SettingsModule { }
