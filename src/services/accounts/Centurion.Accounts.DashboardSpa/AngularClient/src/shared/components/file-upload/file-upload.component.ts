import {
  ChangeDetectionStrategy,
  Component,
  ElementRef,
  EventEmitter,
  HostListener,
  Injector,
  Input,
  OnDestroy,
  OnInit,
  Output,
  ViewChild
} from "@angular/core";
import {environment} from "../../../environments/environment";
import {ControlValueAccessor, NG_VALUE_ACCESSOR} from "@angular/forms";
import {Base64FileResource} from "../../models/base64-file.resource";
import {ControlValueAccessorBase} from "../control-value-accessor-base";

const stopClickPropagation = (e: Event) => e.stopPropagation();

@Component({
  selector: "[appFileUpload]",
  template: `
    <input
      #fileInput
      (change)="handleFileSelected($event)"
      [accept]="supportedFileTypes"
      class="FileUploader-file"
      type="file">
    <ng-content></ng-content>
  `,
  styles: [`input {
    display: none;
  }`],
  changeDetection: ChangeDetectionStrategy.OnPush,
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      multi: true,
      useExisting: FileUploadComponent
    }
  ]
})
export class FileUploadComponent extends ControlValueAccessorBase implements ControlValueAccessor, OnInit, OnDestroy {
  @Input() supportedFileTypes = "";
  @Input() fileUrl: string;
  @Input() clickHostPreventDefault = true;

  @Output()
  fileSelected = new EventEmitter<Base64FileResource>();

  @Output() fileSizeLimitExceeded = new EventEmitter<{ limit: number, size: number }>();

  @ViewChild("fileInput", {static: true}) fileInput: ElementRef<HTMLInputElement>;

  constructor(injector: Injector) {
    super(injector);
  }

  @HostListener("click", ["$event"])
  openFileSelectDialog(e: Event): void {
    if (this.clickHostPreventDefault) {
      e.preventDefault();
    }

    this.fileInput.nativeElement.click();
  }

  handleFileSelected(changeEvent: Event): void {
    const fInput: HTMLInputElement = changeEvent.target as HTMLInputElement;
    if (!fInput.files.length) {
      return;
    }

    const file = fInput.files[0];
    if (file.size > environment.fileSizeLimitBytes) {
      this.fileSizeLimitExceeded.emit({
        size: file.size,
        limit: environment.fileSizeLimitBytes
      });

      return;
    }

    this.dispatchFileSelected(file);
  }

  private dispatchFileSelected(file: File): void {
    const reader = new FileReader();
    reader.addEventListener("loadend", () => {
      const uploadedFile = {
        name: file.name,
        content: reader.result as string,
        contentType: file.type,
        length: file.size
      };

      this.writeValue(uploadedFile);
      this.fileSelected.emit(uploadedFile);
      const input = this.fileInput.nativeElement;
      input.value = "";
      if (!/safari/i.test(navigator.userAgent)) {
        input.type = "";
        input.type = "file";
      }
    });

    reader.readAsDataURL(file);
  }

  ngOnDestroy(): void {
    super.ngOnDestroy();
    this.fileInput.nativeElement.removeEventListener("click", stopClickPropagation);
  }

  ngOnInit(): void {
    super.ngOnInit();
    this.fileInput.nativeElement.addEventListener("click", stopClickPropagation);
  }

}
