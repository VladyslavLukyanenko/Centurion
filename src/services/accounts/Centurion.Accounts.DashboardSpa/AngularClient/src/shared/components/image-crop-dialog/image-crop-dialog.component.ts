import {
  Component,
  OnInit,
  ChangeDetectionStrategy,
  Input,
  EventEmitter,
  Output,
  OnChanges,
  SimpleChanges
} from "@angular/core";
import {ImageCropperResult} from "../cropper/cropper.component";

@Component({
  selector: "app-image-crop-dialog",
  templateUrl: "./image-crop-dialog.component.html",
  styleUrls: ["./image-crop-dialog.component.less"],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ImageCropDialogComponent implements OnInit, OnChanges {
  @Input() isVisible = false;
  @Input() cropImageUrl: string;
  @Input() aspectRatio: number;

  @Output() isVisibleChange = new EventEmitter<boolean>();
  @Output() cropped = new EventEmitter<ImageCropperResult>();

  cropResult: ImageCropperResult;

  croppedBgOptions = {
    aspectRatio: NaN
  };

  constructor() {
  }

  ngOnInit(): void {
    this.croppedBgOptions.aspectRatio = this.aspectRatio;
  }

  ngOnChanges(changes: SimpleChanges): void {
    this.croppedBgOptions.aspectRatio = this.aspectRatio;
  }

  saveChanges(): void {
    this.cropped.emit(this.cropResult);
    this.isVisibleChange.emit(false);
    this.cropResult = null;
  }
}
