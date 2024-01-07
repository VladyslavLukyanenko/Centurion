import {Injectable, NgZone} from "@angular/core";
import {BehaviorSubject, Observable} from "rxjs";
import {GroupedInstanceList, ServerInstancesService} from "../../monitors-panel-api";

@Injectable({
  providedIn: "root"
})
export class ServerNodesProvider {
  private readonly _nodes$ = new BehaviorSubject<GroupedInstanceList[]>([]);
  nodes$: Observable<GroupedInstanceList[]>;

  constructor(private serverInstancesService: ServerInstancesService, private ngZone: NgZone) {
    this.nodes$ = this._nodes$.asObservable();
    this.invalidateNodesList().subscribe();
    this.ngZone.runOutsideAngular(() => {
      setInterval(() => this.invalidateNodesList().subscribe(), 5000);
    });
  }


  invalidateNodesList(): Observable<void> {
    return new Observable<void>(observer => {
      this.serverInstancesService.serverInstancesGetServersList()
        .subscribe(nodes => {
          this._nodes$.next(nodes.payload);
        });
      observer.next();
      observer.complete();
    });
  }
}
