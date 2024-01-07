import {Component, OnInit, ChangeDetectionStrategy, Input} from "@angular/core";
import {environment} from "../../../../environments/environment";

type AllowedHosts = "management";
const hostMap = {
  ["management"]: environment.apiHostName
};

@Component({
  selector: "r-img",
  templateUrl: "./img.component.html",
  styleUrls: ["./img.component.scss"],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ImgComponent implements OnInit {
  @Input()
  src: string;

  @Input()
  alt: string;

  @Input()
  fallbackSrc: string;

  @Input()
  host: AllowedHosts = "management";

  constructor() {
  }

  ngOnInit() {
  }

  getSrc() {
    if (!this.src) {
      return this.fallbackSrc;
    }

    if (this.src.indexOf("data:") > -1) {
      return this.src;
    }

    return hostMap[this.host] + this.src;
  }
}
