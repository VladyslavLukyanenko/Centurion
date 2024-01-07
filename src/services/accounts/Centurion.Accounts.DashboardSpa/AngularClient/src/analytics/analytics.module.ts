import {NgModule} from "@angular/core";
import {SharedModule} from "../shared/shared.module";
import {AnalyticsRoutingModule} from "./analytics-routing.module";
import { AnalyticsDashboardPageComponent } from "./components/analytics-dashboard-page/analytics-dashboard-page.component";
import { AnalyticsGeneralComponent } from "./components/analytics-general/analytics-general.component";
import { AnalyticsDiscordComponent } from "./components/analytics-discord/analytics-discord.component";
import { AnalyticsFiltersComponent } from "./components/analytics-filters/analytics-filters.component";
import { AnalyticsDiscordFiltersComponent } from "./components/analytics-discord-filters/analytics-discord-filters.component";


@NgModule({
  declarations: [AnalyticsDashboardPageComponent, AnalyticsGeneralComponent, AnalyticsDiscordComponent, AnalyticsFiltersComponent, AnalyticsDiscordFiltersComponent],
  imports: [
    SharedModule,
    AnalyticsRoutingModule
  ]
})
export class AnalyticsModule { }
