import {ChangeDetectionStrategy, ChangeDetectorRef, Component, OnInit, ViewContainerRef} from "@angular/core";
import {
  GroupedInstanceList,
  ImagesService,
  ServerInstance,
  ServerInstancesService
} from "../../../monitors-panel-api";
import {BehaviorSubject} from "rxjs";
import {ServerNodesProvider} from "../../services/server-nodes.provider";
import {ImagesProvider} from "../../services/images.provider";
import {DisposableComponentBase} from "../../../shared/components/disposable.component-base";
import {NzMessageService} from "ng-zorro-antd";

@Component({
  selector: "r-dashboard-page",
  templateUrl: "./dashboard-page.component.html",
  styleUrls: ["./dashboard-page.component.scss"],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class DashboardPageComponent extends DisposableComponentBase implements OnInit {
  groups$ = new BehaviorSubject<GroupedInstanceList[]>([]);

  constructor(private imagesService: ImagesService,
              private serverInstancesService: ServerInstancesService,
              private imagesProvider: ImagesProvider,
              private cd: ChangeDetectorRef,
              private messageService: NzMessageService,
              private serverNodesProvider: ServerNodesProvider) {
    super();
  }

  trackNodesBy(idx: number, item: ServerInstance) {
    return item.id;
  }

  async ngOnInit() {
    this.serverNodesProvider.nodes$
      .pipe(this.untilDestroy())
      .subscribe((groups) => {
        this.groups$.next(groups);
        this.cd.detectChanges();
      });

    this.serverNodesProvider.invalidateNodesList().subscribe();
  }

  async runInstance(providerName: string) {
    try {
      await this.asyncTracker.executeRangeAsAsync([
        this.serverInstancesService.serverInstancesStartOrRunServer({providerName: providerName}),
        this.serverNodesProvider.invalidateNodesList()
      ]);
      this.messageService.success(`New server instance of provider "${providerName}" starting. It will be available soon.`);
    } catch (e) {
      let content = "An error occurred on starting new instance.";
      if (e.error && e.error.message) {
        content += " Source message: " + e.error.message;
      }

      this.messageService.error(content);
    }
  }
}
