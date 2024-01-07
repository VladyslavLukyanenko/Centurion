import {ChangeDetectionStrategy, Component, OnInit} from "@angular/core";

@Component({
  selector: "app-forms-page",
  templateUrl: "./forms-page.component.html",
  styleUrls: ["./forms-page.component.less"],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class FormsPageComponent implements OnInit {

  constructor() { }

  ngOnInit(): void {
  }

}
