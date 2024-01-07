import {
  AfterViewInit,
  ChangeDetectionStrategy,
  ChangeDetectorRef,
  Component,
  ElementRef,
  Injector,
  Input,
  OnDestroy,
  OnInit
} from "@angular/core";
import {
  ImageRuntimeInfo,
  ImagesService,
  ServerInstance,
  ServerInstancesService,
  ServerInstanceStatus
} from "../../../monitors-panel-api";
import {AsyncProgressTracker} from "../../../shared/model/async-progress-tracker.model";
import {ImagesProvider} from "../../services/images.provider";
import {NzMessageService, NzModalService} from "ng-zorro-antd";
import {executeBlocking} from "../../services/modal.util";

const statusToColorMap = {
  [ServerInstanceStatus.Running]: "#52c41a",
  [ServerInstanceStatus.Pending]: "#a0d911",
  [ServerInstanceStatus.Unknown]: "#fa541c",
  [ServerInstanceStatus.ShuttingDown]: "#fa8c16",
  [ServerInstanceStatus.Stopping]: "#fa8c16",
  [ServerInstanceStatus.Stopped]: "#d9d9d9",
  [ServerInstanceStatus.Terminated]: "#d9d9d9",
};

const managedInstancesPattern = /instance-\d+/;

@Component({
  selector: "r-node-runtime-info",
  templateUrl: "./node-runtime-info.component.html",
  styleUrls: ["./node-runtime-info.component.scss"],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class NodeRuntimeInfoComponent implements OnInit, OnDestroy, AfterViewInit {
  @Input() node: ServerInstance;
  asyncTrack: AsyncProgressTracker;

  constructor(private imagesService: ImagesService,
              private modal: NzModalService,
              private injector: Injector,
              private cd: ChangeDetectorRef,
              private el: ElementRef,
              private messageService: NzMessageService,
              private serverInstancesService: ServerInstancesService,
              private imagesProvider: ImagesProvider) {
  }

  get isManaged(): boolean {
    return this.node && managedInstancesPattern.test(this.node.id);
  }

  ngOnDestroy(): void {
    this.asyncTrack.dispose();
  }

  ngOnInit(): void {
    this.asyncTrack = new AsyncProgressTracker();
    window.dispatchEvent(new Event("resize"));
    window.document.dispatchEvent(new Event("resize"));
  }


  getColor(): string {
    return statusToColorMap[this.node.status];
  }

  trackImagesBy(idx: number, item: ImageRuntimeInfo) {
    return `${item.containerId}-${item.state}`;
  }

  trackNodesBy(idx: number, item: ServerInstance) {
    return item.id;
  }

  genericErrorNotificationHandler = (err: string) => {
    let message = "An error occurred. ";
    if (err) {
      message += err;
    }

    this.messageService.error(message);
  }

  async stopServer() {
    if (!this.node.isRunning) {
      return;
    }

    const modal = this.modal.confirm({
      nzTitle: "Stop server",
      nzContent: "Are you sure to stop this server?",
      nzOnOk: () => executeBlocking(modal, async () => {
        await this.asyncTrack.executeRangeAsAsync([
          this.serverInstancesService.serverInstancesStopServer(this.node.providerName, this.node.id),
          this.imagesProvider.invalidateImages()
        ]);
        this.messageService.success(`Stopping instance "${this.node.id}". It will take some time.`);
      }, this.genericErrorNotificationHandler)
    });
  }

  async terminateServer() {
    if (!this.node.isRunning && !this.node.isStopped) {
      return;
    }

    const modal = this.modal.confirm({
      nzTitle: "Terminate server",
      nzContent: "Are you sure to terminate this server?",
      nzOnOk: () => executeBlocking(modal, async () => {
        await this.asyncTrack.executeRangeAsAsync([
          this.serverInstancesService.serverInstancesTerminateServer(this.node.providerName, this.node.id),
          this.imagesProvider.invalidateImages()
        ]);
        this.messageService.success(`Terminating instance "${this.node.id}". Server can be listed for some time.`);
      }, this.genericErrorNotificationHandler)
    });
  }

  async shutdown(img: ImageRuntimeInfo) {
    const modal = this.modal.confirm({
      nzTitle: "Shutdown container",
      nzContent: "Are you sure to shutdown this container?",
      nzOnOk: () => executeBlocking(modal, async () => {
        await this.asyncTrack.executeRangeAsAsync([
          this.imagesService.imagesShutdownContainer(this.node.id, this.node.providerName, img.imageInfo.id, img.containerId),
          this.imagesProvider.invalidateImages()
        ]);
        this.messageService.success(`Container stopped and removed.`);
      }, this.genericErrorNotificationHandler)
    });
  }

  async startServer() {
    if (!this.node.isStopped) {
      return;
    }

    await this.asyncTrack.executeRangeAsAsync([
      this.serverInstancesService.serverInstancesStartOrRunServer({
        providerName: this.node.providerName,
        stoppedInstanceId: this.node.id
      }),
      this.imagesProvider.invalidateImages()
    ]);
    this.messageService.success(`Starting instance "${this.node.id}". It will take some time.`);
  }

  ngAfterViewInit(): void {
    // window.dispatchEvent(new Event("resize"));
    // window.document.dispatchEvent(new Event("resize"));
  }
}
