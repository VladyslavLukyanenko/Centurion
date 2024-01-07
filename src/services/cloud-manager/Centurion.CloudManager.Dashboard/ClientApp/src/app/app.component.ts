import {ChangeDetectionStrategy, Component, OnInit} from "@angular/core";
import {BehaviorSubject, Observable} from "rxjs";
import {DisposableComponentBase} from "./shared/components/disposable.component-base";

@Component({
  selector: "r-app",
  templateUrl: "./app.component.html",
  styleUrls: ["./app.component.scss"],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AppComponent extends DisposableComponentBase implements OnInit {
  isProgressVisible: BehaviorSubject<boolean> = new BehaviorSubject<boolean>(false);
  isProgressVisible$: Observable<boolean>;

  constructor() {
    super();
  }

  ngOnInit(): void {

    this.isProgressVisible$ = this.isProgressVisible.asObservable();

    // this.api.activeRequests$
    //   .pipe(this.untilDestroy())
    //   .subscribe(queue => this.isProgressVisible.next(!!queue.length));
  }
}
