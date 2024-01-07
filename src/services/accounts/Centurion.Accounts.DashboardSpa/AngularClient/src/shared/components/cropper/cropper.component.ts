import {Component, ElementRef, EventEmitter, Input, OnInit, Output, ViewChild, ViewEncapsulation} from "@angular/core";
import Cropper from "cropperjs";
import {Subject} from "rxjs";
import {DisposableComponentBase} from "../disposable.component-base";
import {debounceTime} from "rxjs/operators";

export interface ImageCropperResult {
  imageData: Cropper.ImageData;
  cropData: Cropper.CropBoxData;
  blob?: Blob;
  dataUrl?: string;
}

@Component({
  selector: "angular-cropper",
  templateUrl: "./cropper.component.html",
  styleUrls: ["./cropper.component.less"],
  encapsulation: ViewEncapsulation.None
})
export class CropperComponent extends DisposableComponentBase implements OnInit {

  @ViewChild("image") image: ElementRef;

  @Input() imageUrl: any;
  @Input() cropperOptions: Cropper.Options = {};

  @Output() ready = new EventEmitter();
  @Output() cropped = new EventEmitter<ImageCropperResult>();
  @Output() imageLoadFailure = new EventEmitter<Event>();

  cropper: Cropper;
  imageElement: HTMLImageElement;

  private cropDispatcher$ = new Subject<void>();

  constructor() {
    super();
  }

  ngOnInit(): void {
    this.cropDispatcher$
      .pipe(
        this.untilDestroy(),
        debounceTime(500)
      )
      .subscribe(() => this.dispatchCropResult());
  }

  imageLoaded(ev: Event): void {
    const image = ev.target as HTMLImageElement;
    this.imageElement = image;

    if (this.cropperOptions.checkCrossOrigin) {
      image.crossOrigin = "anonymous";
    }

    image.addEventListener("ready", () => {
      this.ready.emit(true);
    });

    this.cropperOptions = Object.assign({
      viewMode: 1,
      dragMode: "move",
      checkCrossOrigin: true
    }, this.cropperOptions);

    this.cropperOptions.crop = () => this.cropDispatcher$.next();

    if (this.cropper) {
      this.cropper.destroy();
      this.cropper = undefined;
    }
    this.cropper = new Cropper(image, this.cropperOptions);
  }

  dispatchCropResult = async () => {
    if (!this.cropped.observers.length) {
      return;
    }

    const imageData = this.cropper.getImageData();
    const cropData = this.cropper.getCropBoxData();
    const canvas = this.cropper.getCroppedCanvas();

    const base64 = await canvas.toDataURL("image/png");
    const blob: Blob = await (new Promise(r => canvas.toBlob(b => r(b))));

    const data: ImageCropperResult = {imageData, cropData, blob, dataUrl: base64};
    this.cropped.emit(data);
  }

  imageLoadError(event: Event): void {
    this.imageLoadFailure.emit(event);
  }
}
