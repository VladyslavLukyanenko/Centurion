import {NgModule} from "@angular/core";
import {CommonModule} from "@angular/common";
import {HttpClientModule} from "@angular/common/http";
import {FormsModule, ReactiveFormsModule} from "@angular/forms";

import {RouterModule} from "@angular/router";

import {LocalDatePipe} from "./pipes/LocalDatePipe";
import {ScrollingModule} from "@angular/cdk/scrolling";
import {DateFromNowPipe} from "./pipes/date-from-now.pipe";
// import {InfiniteScrollModule} from "ngx-infinite-scroll";
import {NgxEchartsModule} from "ngx-echarts";
import {SidebarModule} from "primeng/sidebar";
import {ButtonModule} from "primeng/button";
import {AccordionModule} from "primeng/accordion";
import {CardComponent} from "./components/card/card.component";
import {StatsEntryComponent} from "./components/stats-entry/stats-entry.component";
import {ChartLegendComponent} from "./components/chart-legend/chart-legend.component";
import {HorizontalBarComponent} from "./components/horizontal-bar/horizontal-bar.component";
import {DropdownModule} from "primeng/dropdown";
import {CalendarModule} from "primeng/calendar";
import {ProgressBarModule} from "primeng/progressbar";
import {OverlayPanelModule} from "primeng/overlaypanel";
import {ProgressSpinnerModule} from "primeng/progressspinner";
import {MemberSummaryComponent} from "./components/member-summary/member-summary.component";
import {MenuModule} from "primeng/menu";
import {ConfirmPopupModule} from "primeng/confirmpopup";
import {ConfirmationService} from "primeng/api";
import {DialogModule} from "primeng/dialog";
import {InputSwitchModule} from "primeng/inputswitch";
import {SkeletonModule} from "primeng/skeleton";
import {FieldErrorRequiredComponent} from "./components/errors/field-error-required/field-error-required.component";
import {RadioButtonModule} from "primeng/radiobutton";
import {ToastModule} from "primeng/toast";
import {ConfirmDialogComponent} from "./components/confirm-dialog/confirm-dialog.component";
import {FileUploadComponent} from "./components/file-upload/file-upload.component";
import {CardModule} from "primeng/card";
import {CropperComponent} from "./components/cropper/cropper.component";
import {ImageCropDialogComponent} from "./components/image-crop-dialog/image-crop-dialog.component";
import { InteractivePictureUploaderComponent } from "./components/interactive-picture-uploader/interactive-picture-uploader.component";
import { PermissionDirective } from "./directives/permission.directive";
import { BindLicenseKeyDialogComponent } from "./components/bind-license-key-dialog/bind-license-key-dialog.component";
import { MembersPickerComponent } from "./components/members-picker/members-picker.component";
import { AlertComponent } from "./components/alert/alert.component";
import {TooltipModule} from "primeng/tooltip";

const componentsModules = [
  NgxEchartsModule,
  // InfiniteScrollModule,
  ScrollingModule,
  SidebarModule,
  CalendarModule,
  ButtonModule,
  TooltipModule,
  AccordionModule,
  DropdownModule,
  ProgressBarModule,
  ProgressSpinnerModule,
  OverlayPanelModule,
  MenuModule,
  ConfirmPopupModule,
  DialogModule,
  InputSwitchModule,
  SkeletonModule,
  RadioButtonModule,
  ToastModule,
  CardModule
];

const sharedSystemModules = [
  CommonModule,
  HttpClientModule,
  ReactiveFormsModule,
  FormsModule
];

const sharedDeclarations = [
  LocalDatePipe,
  DateFromNowPipe,
  CardComponent,
  StatsEntryComponent,
  ChartLegendComponent,
  HorizontalBarComponent,
  MemberSummaryComponent,
  FieldErrorRequiredComponent,


  ConfirmDialogComponent,
  FileUploadComponent,

  CropperComponent,
  ImageCropDialogComponent,
  InteractivePictureUploaderComponent
];

@NgModule({
  imports: [
    RouterModule,

    ...sharedSystemModules,
    ...componentsModules
  ],
  declarations: [
    ...sharedDeclarations,
    PermissionDirective,
    BindLicenseKeyDialogComponent,
    MembersPickerComponent,
    AlertComponent,
  ],
  entryComponents: [],
  exports: [
    ...sharedSystemModules,
    ...componentsModules,
    ...sharedDeclarations,
    PermissionDirective,
    BindLicenseKeyDialogComponent,
    MembersPickerComponent,
    AlertComponent,
  ],
  providers: [
    ConfirmationService
  ]
})
export class SharedModule {
}
