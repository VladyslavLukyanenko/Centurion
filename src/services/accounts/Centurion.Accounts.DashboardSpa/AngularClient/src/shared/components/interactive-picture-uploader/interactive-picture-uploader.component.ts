import {
  ChangeDetectionStrategy,
  Component,
  EventEmitter,
  Injector,
  Input,
  OnChanges,
  Output,
  SimpleChanges
} from "@angular/core";
import {BehaviorSubject, Subscription} from "rxjs";
import {ImageCropperResult} from "../cropper/cropper.component";
import {ControlValueAccessorBase} from "../control-value-accessor-base";
import {NG_VALUE_ACCESSOR} from "@angular/forms";
import {Base64FileResource} from "../../models/base64-file.resource";
import {Observable} from "rxjs/internal/Observable";

@Component({
  selector: "app-interactive-picture-uploader",
  templateUrl: "./interactive-picture-uploader.component.html",
  styleUrls: ["./interactive-picture-uploader.component.less"],
  changeDetection: ChangeDetectionStrategy.OnPush,
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      multi: true,
      useExisting: InteractivePictureUploaderComponent
    }
  ]
})
export class InteractivePictureUploaderComponent extends ControlValueAccessorBase implements OnChanges {
  @Input() source: string;
  @Input() aspectRatio = 1;
  @Input() editText = "Upload";
  @Input() label = "Picture";
  @Input() invalidationToken$: Observable<any>;
  @Output() pictureSelected = new EventEmitter<Base64FileResource>();
  cropDialogData$ = new BehaviorSubject<string>(null);
  changedPic$ = new BehaviorSubject<string>(null);

  private invalidationSub: Subscription;
  constructor(injector: Injector) {
    super(injector);
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes.source && changes.source.currentValue !== changes.source.previousValue) {
      this.clear();
    }

    if (changes.invalidationToken$) {
      this.invalidationSub?.unsubscribe();
      if (this.invalidationToken$) {
        this.invalidationSub = this.invalidationToken$.pipe(this.untilDestroy()).subscribe(() => this.clear());
      }
    }
  }

  get hasImage(): boolean {
    return !!this.changedPic$.value || !!this.source;
  }


  saveCropChanges(e: ImageCropperResult): void {
    const data: Base64FileResource = {
      length: e.blob.size,
      content: e.dataUrl,
      contentType: e.blob.type
    };
    this.writeValue(data);
    this.pictureSelected.emit(data);
    this.cropDialogData$.next(null);
    this.changedPic$.next(e.dataUrl);
  }

  private clear(): void {
    this.changedPic$.next(null);
    this.cropDialogData$.next(null);
  }
}
