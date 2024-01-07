import {ImageRuntimeInfo, ServerInstance, ServerInstanceStatus} from "../../monitors-panel-api";
import {BehaviorSubject, Observable} from "rxjs";
import {distinctUntilChanged} from "rxjs/operators";

export class ObservableServerInstance {
  private _isAvailable$ = new BehaviorSubject<boolean>(false);
  private _lastChecked$ = new BehaviorSubject<Date>(new Date(0));
  private _status$ = new BehaviorSubject<ServerInstanceStatus>(ServerInstanceStatus.Unknown);
  private _isRunning$ = new BehaviorSubject<boolean>(false);
  private _isStopped$ = new BehaviorSubject<boolean>(false);
  private _isIdle$ = new BehaviorSubject<boolean>(false);
  private _additionalStats$ = new BehaviorSubject<string>("");
  private _images$ = new BehaviorSubject<ImageRuntimeInfo[]>([]);

  constructor(source: ServerInstance) {
    this.source = source;

   this.isAvailable$ = this._isAvailable$
     .asObservable()
     .pipe(distinctUntilChanged());

   this.lastChecked$ = this._lastChecked$
     .asObservable()
     .pipe(distinctUntilChanged());

   this.status$ = this._status$
     .asObservable()
     .pipe(distinctUntilChanged());

   this.isRunning$ = this._isRunning$
     .asObservable()
     .pipe(distinctUntilChanged());

   this.isStopped$ = this._isStopped$
     .asObservable()
     .pipe(distinctUntilChanged());

   this.isIdle$ = this._isIdle$
     .asObservable()
     .pipe(distinctUntilChanged());

   this.additionalStats$ = this._additionalStats$
     .asObservable()
     .pipe(distinctUntilChanged());

   this.images$ = this._images$
     .asObservable()
     .pipe(distinctUntilChanged());
  }

  source: ServerInstance;
  isAvailable$: Observable<boolean>;
  lastChecked$: Observable<Date>;
  status$: Observable<ServerInstanceStatus>;
  isRunning$: Observable<boolean>;
  isStopped$: Observable<boolean>;
  isIdle$: Observable<boolean>;
  additionalStats$: Observable<string>;

  images$: Observable<ImageRuntimeInfo[]>;

  refreshFrom(source: ServerInstance) {
    this._isAvailable$.next(source.isAvailable);
    this._lastChecked$.next(source.lastChecked);
    this._status$.next(source.status);
    this._isRunning$.next(source.isRunning);
    this._isStopped$.next(source.isStopped);
    this._isIdle$.next(source.isIdle);

    const keys = Object.keys(source.additionalStats);
    this._additionalStats$.next(keys.map(k => `${k}: ${source.additionalStats[k]}`).join("\n"));
  }
}
