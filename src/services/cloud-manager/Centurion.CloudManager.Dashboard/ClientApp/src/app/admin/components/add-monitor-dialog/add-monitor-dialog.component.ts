import {ChangeDetectionStrategy, Component, OnInit} from "@angular/core";
import {FormBuilder, FormGroup, Validators} from "@angular/forms";
import {NzModalRef} from "ng-zorro-antd";
import {ImageType} from "../../../monitors-panel-api";

@Component({
  selector: "r-add-monitor-dialog",
  templateUrl: "./add-monitor-dialog.component.html",
  styleUrls: ["./add-monitor-dialog.component.scss"],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AddMonitorDialogComponent implements OnInit {
  imageTypes = [ImageType.Monitor, ImageType.Publisher];

  form: FormGroup;
  constructor(private fb: FormBuilder, private modalRef: NzModalRef) { }
  ngOnInit(): void {
    this.form = this.fb.group({
      imageName: [null, [Validators.required]],
      imageType: [null, [Validators.required]],
      requiredSpawnParameters: [null, [Validators.required]],
      slug: [null, [Validators.required]]
    });
  }

  create() {
    if (!this.form.valid) {
      return;
    }

    this.modalRef.triggerOk();
  }
}
