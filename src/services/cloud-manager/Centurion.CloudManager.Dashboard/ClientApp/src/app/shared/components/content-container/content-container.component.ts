import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';

@Component({
  selector: 'r-content-container',
  templateUrl: './content-container.component.html',
  styleUrls: ['./content-container.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ContentContainerComponent implements OnInit {

  constructor() { }

  ngOnInit() {
  }

}
