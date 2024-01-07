import {ChangeDetectionStrategy, Component, OnInit} from '@angular/core';

@Component({
  selector: 'app-tickets-page',
  templateUrl: './tickets-page.component.html',
  styleUrls: ['./tickets-page.component.less'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class TicketsPageComponent implements OnInit {

  constructor() { }

  ngOnInit(): void {
  }

}
