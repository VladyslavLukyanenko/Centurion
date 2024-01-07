import {
  ChangeDetectionStrategy,
  ChangeDetectorRef,
  Component,
  EventEmitter,
  forwardRef,
  Injector,
  Input,
  OnChanges,
  Output,
  SimpleChanges
} from "@angular/core";
import {environment} from "../../../../environments/environment";
import {NotificationService} from "../../../core/services/notifications/notification.service";
import {TranslateService} from "@ngx-translate/core";
import {Base64FileData} from "../../model/base64-file.data";
import {NG_VALUE_ACCESSOR} from "@angular/forms";
import {FileUploadBase} from "../file-upload-base";

@Component({
  selector: "r-file-upload",
  templateUrl: "./file-upload.component.html",
  styleUrls: ["./file-upload.component.scss"],
  changeDetection: ChangeDetectionStrategy.OnPush,
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => FileUploadComponent),
      multi: true
    }
  ]
})
export class FileUploadComponent extends FileUploadBase implements OnChanges {
  @Input()
  supportedFileTypes = "";

  @Input()
  fileUrl: string;

  @Input()
  uploadBtnText: string = "fileUploader.btnText";

  @Output()
  fileProcessed = new EventEmitter<Base64FileData>();

  @Output()
  fileSelected = new EventEmitter<File>();

  displayFileName: string;

  constructor(
    private changeDetector: ChangeDetectorRef,
    private readonly notificationService: NotificationService,
    private readonly translateService: TranslateService,
    injector: Injector) {
    super(injector);
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
    this.fileProcessed.emit(data);
    this.displayFileName = file.name;
    this.changeDetector.detectChanges();
  }

  protected onFileSelected(file: File) {
    this.fileSelected.emit(file);
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes.fileUrl && changes.fileUrl.currentValue) {
      this.displayFileName = this.getDisplayFileName();
    }
  }

  private getDisplayFileName() {
    let startIdx;
    if (this.fileUrl && (startIdx = this.fileUrl.lastIndexOf("/")) !== -1) {
      return this.fileUrl.substring(startIdx + 1);
    }

    return this.fileUrl;
  }
}
