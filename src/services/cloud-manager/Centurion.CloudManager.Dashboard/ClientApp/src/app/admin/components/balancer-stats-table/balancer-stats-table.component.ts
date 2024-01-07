import {Component, OnInit, ChangeDetectionStrategy, Input} from '@angular/core';
import {ComponentStatsEntry} from "../../../monitors-panel-api";

@Component({
  selector: 'r-balancer-stats-table',
  templateUrl: './balancer-stats-table.component.html',
  styleUrls: ['./balancer-stats-table.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class BalancerStatsTableComponent implements OnInit {

  @Input() stats: ComponentStatsEntry[]
  constructor() { }

  ngOnInit(): void {
  }

  parseBalancerEntries(stats: string): BalancerStatsEntry[] {
    return JSON.parse(stats);
  }
}

interface BalancerStatsEntry {
  slug: string;
  webhookUrl: string;
}
