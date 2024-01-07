import {Injectable} from "@angular/core";
import {ImageInfo, ImagesService, ServerInstance} from "../../monitors-panel-api";
import {BehaviorSubject, Observable} from "rxjs";

export interface ServerStatsDict {
  [serverId: number]: ServerInstance;
}

@Injectable({
  providedIn: "root"
})
export class ImagesProvider {
  private _images$ = new BehaviorSubject<ImageInfo[]>([]);
  images$: Observable<ImageInfo[]>;
  constructor(private imagesService: ImagesService) {
    this.images$ = this._images$.asObservable();
    this.invalidateImages().subscribe();
  }

  invalidateImages(): Observable<void> {
    return new Observable<void>(observer => {
      this.imagesService.imagesGetAvailableImagesList().subscribe(images => {
        this._images$.next(images.payload);
        observer.next();
        observer.complete();
      });
    });
  }
}
