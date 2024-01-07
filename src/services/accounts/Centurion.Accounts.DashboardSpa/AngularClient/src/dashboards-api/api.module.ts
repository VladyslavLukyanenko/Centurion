import { NgModule, ModuleWithProviders, SkipSelf, Optional } from '@angular/core';
import { Configuration } from './configuration';
import { HttpClient } from '@angular/common/http';

import { AnalyticsService } from './api/analytics.service';
import { AuditService } from './api/audit.service';
import { BotsLicenseKeysService } from './api/bots-license-keys.service';
import { CountriesService } from './api/countries.service';
import { DashboardsService } from './api/dashboards.service';
import { FormValuesService } from './api/form-values.service';
import { FormsService } from './api/forms.service';
import { GlobalSearchService } from './api/global-search.service';
import { LicenseKeysService } from './api/license-keys.service';
import { MemberRoleBindingsService } from './api/member-role-bindings.service';
import { MemberRolesService } from './api/member-roles.service';
import { PaymentsService } from './api/payments.service';
import { PermissionsService } from './api/permissions.service';
import { PlansService } from './api/plans.service';
import { ReleasesService } from './api/releases.service';
import { StripeWebHooksService } from './api/stripe-web-hooks.service';
import { TimeZonesService } from './api/time-zones.service';
import { UpdatesService } from './api/updates.service';
import { UsersService } from './api/users.service';

@NgModule({
  imports:      [],
  declarations: [],
  exports:      [],
  providers: []
})
export class ApiModule {
    public static forRoot(configurationFactory: () => Configuration): ModuleWithProviders<ApiModule> {
        return {
            ngModule: ApiModule,
            providers: [ { provide: Configuration, useFactory: configurationFactory } ]
        };
    }

    constructor( @Optional() @SkipSelf() parentModule: ApiModule,
                 @Optional() http: HttpClient) {
        if (parentModule) {
            throw new Error('ApiModule is already loaded. Import in your base AppModule only.');
        }
        if (!http) {
            throw new Error('You need to import the HttpClientModule in your AppModule! \n' +
            'See also https://github.com/angular/angular/issues/20575');
        }
    }
}
