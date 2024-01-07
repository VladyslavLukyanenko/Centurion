import {Injectable} from "@angular/core";
import {BehaviorSubject, Observable} from "rxjs";
import {Title} from "@angular/platform-browser";
import {TranslateService} from "@ngx-translate/core";
import {environment} from "../../../environments/environment";
import {MessageBusService} from "./messaging/message-bus.service";

@Injectable({
  providedIn: "root"
})
export class ToolbarService {
  private readonly titleSubj: BehaviorSubject<string>;
  private lastTitleKey: string;

  constructor(private title: Title,
              private translate: TranslateService,
              private messageBus: MessageBusService) {
    this.titleSubj = new BehaviorSubject<string>("");
  }

  get titleToken$(): Observable<string> {
    return this.titleSubj.asObservable();
  }

  get currentTitleToken(): string {
    return this.titleSubj.getValue();
  }

  setTitle(value: string) {
    this.titleSubj.next(value);
    const message = new TitleChanged(this.lastTitleKey, value);
    this.lastTitleKey = value;
    this.updateDocumentTitle();
    this.messageBus.broadcast(TitleChanged, message);
  }

  private updateDocumentTitle() {
    if (!this.lastTitleKey) {
      return;
    }

    this.translate.get(this.lastTitleKey).subscribe(title => {
      this.title.setTitle(title + ` | ${environment.publicProjectName}`);
    });
  }
}

export class TitleChanged {
  constructor(readonly prevTitle: string, readonly newTitle: string) {}
}
