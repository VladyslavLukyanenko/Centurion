import {ChangeDetectionStrategy, Component, Input, OnInit} from "@angular/core";
import {FormBuilder, FormGroup, Validators} from "@angular/forms";
import {NzModalRef} from "ng-zorro-antd";
import {GroupedInstanceList, ImageInfo} from "../../../monitors-panel-api";
import {ServerNodesProvider} from "../../services/server-nodes.provider";
import {Observable} from "rxjs";

@Component({
  selector: "r-spawn-monitor-dialog",
  templateUrl: "./spawn-monitor-dialog.component.html",
  styleUrls: ["./spawn-monitor-dialog.component.scss"],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class SpawnMonitorDialogComponent implements OnInit {
  @Input() monitorImage: ImageInfo;

  form: FormGroup;
  groups$: Observable<GroupedInstanceList[]>;
  constructor(private fb: FormBuilder,
              private modalRef: NzModalRef,
              private nodesProvider: ServerNodesProvider) {
    this.groups$ = this.nodesProvider.nodes$;
  }

  ngOnInit(): void {
    const parameters = {};
    for (const c of this.monitorImage.requiredSpawnParameters) {
      parameters[c] = this.fb.control(null, [Validators.required]);
    }

    this.form = this.fb.group({
      imageId: [this.monitorImage.id, [Validators.required]],
      serverId: [],
      parameters: this.fb.group(parameters),
    });
  }

  create() {
    if (!this.form.valid) {
      return;
    }

    this.modalRef.triggerOk();
  }
}
