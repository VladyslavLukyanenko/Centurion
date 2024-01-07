import { Injectable } from "@angular/core";
import * as ClipboardPolyfill from "clipboard-polyfill";

@Injectable({
  providedIn: "root"
})
export class ClipboardService {

  constructor() {
  }

  async writeText(text: string): Promise<void> {
    await ClipboardPolyfill.writeText(text);
  }
}
