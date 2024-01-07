import {Injectable, Type} from "@angular/core";
import {Subscription} from "rxjs";

@Injectable({
  providedIn: "root"
})
export class MessageBusService {
  private holders: MessageEventHandlersHolder[] = [];
  subscribe<T>(messageType: Type<T>, callback: (arg: T) => void): Subscription {
    let holder = this.holders.find(_ => _.messageType === messageType);
    if (!holder) {
      holder = new MessageEventHandlersHolder(messageType);
      this.holders.push(holder);
    }

    const handler = new MessageBusEventHandler(messageType, callback);
    holder.add(handler);
    return new Subscription(() => holder.remove(handler));
  }

  broadcast<T>(messageType: Type<T>, arg: T) {
    const holder = this.holders.find(_ => _.messageType === messageType);
    if (!holder) {
      return;
    }

    for (let i = 0; i < holder.handlers.length; i++) {
      const cur = holder.handlers[i];
      cur.callback(arg);
    }
  }
}

class MessageEventHandlersHolder {
  readonly handlers: MessageBusEventHandler[];
  constructor(readonly messageType: Type<any>) {
    this.handlers = [];
  }

  add(handler: MessageBusEventHandler) {
    this.handlers.push(handler);
  }

  remove(handler: MessageBusEventHandler) {
    const idx = this.handlers.indexOf(handler);
    if (idx > -1) {
      this.handlers.splice(idx, 1);
    }
  }
}

class MessageBusEventHandler {
  constructor(readonly messageType: Type<any>, readonly callback: Function) {}
}
