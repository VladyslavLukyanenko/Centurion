import {ChangeDetectionStrategy, Component, OnInit} from '@angular/core';

@Component({
  selector: 'app-embeds-page',
  templateUrl: './embeds-page.component.html',
  styleUrls: ['./embeds-page.component.less'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class EmbedsPageComponent implements OnInit {

  constructor() { }

  ngOnInit(): void {
  }

}
