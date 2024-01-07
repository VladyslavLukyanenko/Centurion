import {
  ChangeDetectionStrategy,
  ChangeDetectorRef,
  Component,
  EventEmitter,
  forwardRef,
  Injector,
  Input,
  Output
} from "@angular/core";
import {NG_VALUE_ACCESSOR} from "@angular/forms";
import {environment} from "../../../../environments/environment";
import {TranslateService} from "@ngx-translate/core";
import {NotificationService} from "../../../core/services/notifications/notification.service";
import {Base64FileData} from "../../model/base64-file.data";
import {FileUploadBase} from "../file-upload-base";

@Component({
  selector: "r-photo-upload",
  templateUrl: "./photo-upload.component.html",
  styleUrls: ["./photo-upload.component.scss"],
  changeDetection: ChangeDetectionStrategy.OnPush,
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => PhotoUploadComponent),
      multi: true
    }
  ]
})
export class PhotoUploadComponent extends FileUploadBase {
  supportedImageTypes = environment.supportedImageTypes;

  @Input() showPreview: boolean = true;
  @Input() previewSrc: string | any;
  @Input() label: string;
  @Input() buttonText: string;
  @Input() readOnly: boolean = false;
  @Input() showCloseBtn: boolean = false;
  @Output() buttonClicked: EventEmitter<any> = new EventEmitter<any>();
  @Output() fileSelect: EventEmitter<Base64FileData> = new EventEmitter<Base64FileData>();

  constructor(
    private changeDetector: ChangeDetectorRef,
    private readonly notificationService: NotificationService,
    private readonly translateService: TranslateService,
    injector: Injector) {
    super(injector);
  }

  handleFileSelected(changeEvent: Event) {
    if (this.readOnly) {
      return;
    }

    super.handleFileSelected(changeEvent);
  }

  protected onFileSizeExceeded(file: File): boolean {
    const params = {
      size: environment.fileSizeLimitBytes / 1024 / 1024
    };

    this.translateService.get("globalErrors.UploadFileSizeLimitExceeded", params)
      .subscribe(message => {
        this.notificationService.error(message);
      });

    return true;
  }
  protected onFileReadEnd(data: Base64FileData, file: File) {
    this.fileSelect.emit(data);

    if (this.showPreview) {
      this.previewSrc = data.content;
      this.changeDetector.detectChanges();
    }
  }

  getPreviewSrc() {
    return this.previewSrc || "/assets/back-up.svg";
  }
}
