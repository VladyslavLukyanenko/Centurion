import {Component, OnInit, ChangeDetectionStrategy, Input} from '@angular/core';
import {ComponentStatsEntry} from "../../../monitors-panel-api";
// import * as noPicAvailable from "assets/no-pic-available.jpg";

@Component({
  selector: 'r-monitor-stats-table',
  templateUrl: './monitor-stats-table.component.html',
  styleUrls: ['./monitor-stats-table.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class MonitorStatsTableComponent implements OnInit {
  fallbackPic = "assets/no-pic-available.jpg";
  @Input() stats: ComponentStatsEntry[]
  constructor() { }

  ngOnInit(): void {
  }


  parseMonitorEntries(stats: string): MonitorStatsEntry[] {
    return JSON.parse(stats);
  }
}


interface MonitorStatsEntry {
  productUrl: string;
  productPic: string;
  title: string;
}
