import {ChangeDetectionStrategy, Component, OnInit, Type, ViewContainerRef} from "@angular/core";
import {ImagesProvider} from "../../services/images.provider";
import {ServerNodesProvider} from "../../services/server-nodes.provider";
import {
  CreateImageCommand,
  GroupedInstanceList,
  ImageInfo,
  ImagesService,
  SpawnImageCommand
} from "../../../monitors-panel-api";
import {Observable} from "rxjs";
import {AsyncProgressTracker} from "../../../shared/model/async-progress-tracker.model";
import {NzMessageService, NzModalService} from "ng-zorro-antd";
import {AddMonitorDialogComponent} from "../add-monitor-dialog/add-monitor-dialog.component";
import {SpawnMonitorDialogComponent} from "../spawn-monitor-dialog/spawn-monitor-dialog.component";
import {FormGroup} from "@angular/forms";

@Component({
  selector: "r-server-images-page",
  templateUrl: "./server-images-page.component.html",
  styleUrls: ["./server-images-page.component.scss"],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ServerImagesPageComponent implements OnInit {
  images$: Observable<ImageInfo[]>;
  nodes$: Observable<GroupedInstanceList[]>;

  imagesAsyncIndicator = new AsyncProgressTracker();

  constructor(private imagesProvider: ImagesProvider,
              private serverNodesProvider: ServerNodesProvider,
              private imagesService: ImagesService,
              private modalService: NzModalService,
              private viewContainerRef: ViewContainerRef,
              private messageService: NzMessageService) {
  }

  ngOnInit() {
    this.images$ = this.imagesProvider.images$;
    this.nodes$ = this.serverNodesProvider.nodes$;
  }

  stringifyParams(requiredSpawnParameters: string[]) {
    return requiredSpawnParameters.join(", ");
  }

  showSpawnImageWindow(img: ImageInfo) {
    this.showModal(SpawnMonitorDialogComponent, "Spawn monitor image container", {monitorImage: img},
      async form => {
        const value = form.value;
        const cmd: SpawnImageCommand = {
          imageId: value.imageId,
          parameters: value.parameters,
          serverId: value.serverId
        };
        await this.imagesAsyncIndicator.executeRangeAsAsync([
          this.imagesService.imagesSpawnImage(cmd),
          this.imagesProvider.invalidateImages()
        ]);
      }, () =>
        this.messageService.error("Can't spawn image. Please make sure there are available idle servers"));
  }

  addNewMonitor(): void {
    this.showModal(AddMonitorDialogComponent, "Add new image", null, async form => {
      const value = form.value;
      const cmd: CreateImageCommand = {
        imageName: value.imageName,
        requiredSpawnParameters: value.requiredSpawnParameters.split("\n"),
        slug: value.slug,
        imageType: value.imageType
      };
      await this.imagesAsyncIndicator.executeRangeAsAsync([
        this.imagesService.imagesCreate(cmd),
        this.imagesProvider.invalidateImages()
      ]);
    }, () =>
      this.messageService.error("Can't add new monitor. Please check image name, tag"));
  }

  async removeImage(img: ImageInfo): Promise<void> {
    await this.imagesAsyncIndicator.executeRangeAsAsync([
      this.imagesService.imagesRemove(img.id),
      this.imagesProvider.invalidateImages()
    ]);
  }

  private showModal(componentType: Type<any>, title: string, componentsParams: any,
                    handler: (form: FormGroup) => Promise<any>, handleError: (details?: string) => void = null) {
    const modal = this.modalService.create({
      nzContent: componentType,
      nzTitle: title,
      nzComponentParams: componentsParams,
      nzViewContainerRef: this.viewContainerRef,
      nzOnOk: (cmp) => {
        return new Promise(async (resolve, reject) => {
          modal.updateConfig({
            nzOkLoading: true,
            nzCancelDisabled: true,
            nzClosable: false
          });
          try {
            if (!cmp.form.valid) {
              return reject(false);
            }

            cmp.form.disable();
            await handler(cmp.form);

            resolve(true);
          } catch (e) {
            if (handleError) {
              let details = null;
              if (e.error && e.error.message) {
                details = e.error.message;
              }
              handleError(details);
            }

            reject(false);
          } finally {
            cmp.form.enable();
            modal.updateConfig({
              nzOkLoading: false,
              nzCancelDisabled: false,
              nzClosable: true
            });
          }
        });
      }
    });
  }
}
