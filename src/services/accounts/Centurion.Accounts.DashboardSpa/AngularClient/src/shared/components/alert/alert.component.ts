import {Component, OnInit, ChangeDetectionStrategy, Input, Output, EventEmitter} from "@angular/core";

@Component({
  selector: "app-alert",
  templateUrl: "./alert.component.html",
  styleUrls: ["./alert.component.less"],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AlertComponent implements OnInit {
  @Input() title: string;
  @Input() message: string;
  @Input() isVisible = false;
  @Output() isVisibleChange = new EventEmitter<boolean>();

  constructor() { }

  ngOnInit(): void {
  }

}
