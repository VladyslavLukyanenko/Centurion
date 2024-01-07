import {Injectable} from "@angular/core";
import {BehaviorSubject, Observable} from "rxjs";
import {Title} from "@angular/platform-browser";
import {environment} from "../../environments/environment";
import {MessageBusService} from "./messaging/message-bus.service";

@Injectable({
  providedIn: "root"
})
export class ToolbarService {
  private readonly titleSubj: BehaviorSubject<string>;
  private lastTitleKey: string | null = null;

  constructor(private title: Title,
              private messageBus: MessageBusService) {
    this.titleSubj = new BehaviorSubject<string>("");
  }

  get titleToken$(): Observable<string> {
    return this.titleSubj.asObservable();
  }

  get currentTitleToken(): string {
    return this.titleSubj.getValue();
  }

  setTitle(value: string): void {
    this.titleSubj.next(value);
    const message = new TitleChanged(this.lastTitleKey, value);
    this.lastTitleKey = value;
    this.updateDocumentTitle();
    this.messageBus.broadcast(TitleChanged, message);
  }

  private updateDocumentTitle(): void {
    if (!this.lastTitleKey) {
      return;
    }

    this.title.setTitle(this.lastTitleKey + ` | ${environment.publicProjectName}`);
  }
}

export class TitleChanged {
  constructor(readonly prevTitle: string | null, readonly newTitle: string) {}
}
