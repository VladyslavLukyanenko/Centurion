import {Injectable} from "@angular/core";
import {BehaviorSubject, Observable} from "rxjs";
import {map} from "rxjs/operators";

@Injectable({
  providedIn: "root"
})
export class ActivityTrackerService {
  private _pendingRequests = new BehaviorSubject<any[]>([]);

  pendingRequests$: Observable<number>;
  constructor() {
    this.pendingRequests$ = this._pendingRequests.asObservable()
      .pipe(map(r => r.length));
  }

  push(r: any) {
    this._pendingRequests.next([
      ...this._pendingRequests.getValue(),
      r
    ]);
  }

  remove(r: any) {
    const pendingRequests = this._pendingRequests.getValue().slice();
    const idx = pendingRequests.indexOf(r);
    if (idx !== -1) {
      pendingRequests.splice(idx, 1);
      this._pendingRequests.next(pendingRequests);
    }
  }
}
