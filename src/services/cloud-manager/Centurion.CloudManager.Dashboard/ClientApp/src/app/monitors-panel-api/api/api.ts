export * from './component-stats.service';
import { ComponentStatsService } from './component-stats.service';
export * from './images.service';
import { ImagesService } from './images.service';
export * from './server-instances.service';
import { ServerInstancesService } from './server-instances.service';
export const APIS = [ComponentStatsService, ImagesService, ServerInstancesService];
