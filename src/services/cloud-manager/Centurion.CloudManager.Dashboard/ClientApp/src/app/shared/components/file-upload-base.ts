import {Injector} from "@angular/core";
import {ControlValueAccessorBase} from "./control-value-accessor-base";
import {Base64FileData} from "../model/base64-file.data";
import {environment} from "../../../environments/environment";

export abstract class FileUploadBase extends ControlValueAccessorBase {
  protected constructor(injector: Injector) {
    super(injector);
  }

  /**
   * Called when selected file exceeds size from config. When true - stop uploading, otherwise - ignore and continue.
   * @param file Selected file
   */
  protected abstract onFileSizeExceeded(file: File): boolean;

  /**
   * Called after file selected and end read.
   * @param data Created data from source file
   * @param file Selected file
   */
  protected abstract onFileReadEnd(data: Base64FileData, file: File);

  /**
   * Called after file selected and before read.
   * @param file Selected file
   */
  protected onFileSelected(file: File) {
  }

  writeValue(obj: any): void {
    // NOOP
  }

  handleFileSelected(changeEvent: Event) {
    const fInput: HTMLInputElement = <HTMLInputElement>changeEvent.target;
    if (!fInput.files.length) {
      return;
    }

    const file = fInput.files[0];
    if (file.size > environment.fileSizeLimitBytes) {
      if (this.onFileSizeExceeded(file)) {
        return;
      }
    }

    if (this.valueChangeObserver) {
      this.valueChangeObserver(file);
    }

    this.dispatchFileSelected(file);
  }

  private dispatchFileSelected(file: File) {
    this.onFileSelected(file);
    const reader = new FileReader();
    reader.addEventListener("loadend", () => {
      const data: Base64FileData = {
        name: file.name,
        content: reader.result as string,
        contentType: file.type,
        length: file.size
      };

      this.control.setValue(data);
      this.onFileReadEnd(data, file);
    });

    reader.readAsDataURL(file);
  }
}
