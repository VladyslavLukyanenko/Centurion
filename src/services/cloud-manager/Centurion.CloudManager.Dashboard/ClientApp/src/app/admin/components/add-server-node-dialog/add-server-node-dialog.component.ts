import { Component, OnInit, ChangeDetectionStrategy } from "@angular/core";
import {FormBuilder, FormGroup, Validators} from "@angular/forms";
import {NzModalRef} from "ng-zorro-antd";
import {SupportedHostingTargets} from "../../../monitors-panel-api";

@Component({
  selector: "r-add-server-node-dialog",
  templateUrl: "./add-server-node-dialog.component.html",
  styleUrls: ["./add-server-node-dialog.component.scss"],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AddServerNodeDialogComponent implements OnInit {
  form: FormGroup;
  supportedTargets = [SupportedHostingTargets.Monitors, SupportedHostingTargets.Publishers];
  constructor(private fb: FormBuilder, private modalRef: NzModalRef) { }
  ngOnInit(): void {
    this.form = this.fb.group({
      endPoint: [null, [Validators.required]],
      username: [],
      password: [],
      supportedTargets: []
    });
  }

  create() {
    if (!this.form.valid) {
      return;
    }

    this.modalRef.triggerOk();
  }

}
