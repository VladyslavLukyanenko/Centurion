import {Injectable} from "@angular/core";

@Injectable({
  providedIn: "root"
})
export class GlobalShadowService {
  private shadow: HTMLElement | null = null;

  show(zIdx: number = 10) {
    if (!this.shadow) {
      this.shadow = document.createElement("div");
      this.shadow.classList.add("GlobalShadow");
      this.shadow.addEventListener("click", this.hide);
    }

    if (this.shadow.parentElement) {
      return;
    }

    if (zIdx != null && !isNaN(zIdx)) {
      this.shadow.style.zIndex = String(zIdx);
    }

    document.body.appendChild(this.shadow);
  }

  hide = () => {
    if (!this.shadow?.parentElement) {
      return;
    }

    document.body.removeChild(this.shadow);
  }
}
