import {AppPermissions} from "../models/permissions.model";
import {Injectable} from "@angular/core";
import {PermissionsSelector} from "./identity.service";

export interface MenuItem {
  route: string;
  title: string;
  badge?: string;
  icon: string;
  permissions: PermissionsSelector | string;
}

export interface MenuItemGroup {
  name: string;
  items: MenuItem[];
}

const regularMenuItems: MenuItem[] = [
  {
    route: "/purchases",
    title: "Purchases",
    icon: "#analytics",
    permissions: null
  },
  {
    route: "/analytics",
    title: "Analytics",
    icon: "#analytics",
    permissions: {any: [AppPermissions.AnalyticsGeneralRead, AppPermissions.AnalyticsDiscordRead]}
  },
  {
    route: "/plans",
    title: "Plans",
    icon: "#products",
    permissions: AppPermissions.PlansManage
  },
  {
    route: "/releases",
    title: "Releases",
    icon: "#releases",
    badge: "Pro",
    permissions: AppPermissions.ReleaseManage
  },
  {
    route: "/licenses",
    title: "Licenses",
    icon: "#licenses",
    permissions: AppPermissions.LicenseKeysManage
  },
  {
    route: "/tickets",
    title: "Tickets",
    icon: "#tickets",
    permissions: null
  },
  {
    route: "/staff",
    title: "Staff",
    icon: "#staff",
    permissions: {all: [AppPermissions.StaffManage, AppPermissions.RolesManage]}
  },
  {
    route: "/forms",
    title: "Forms",
    icon: "#orders",
    permissions: null
  }];

const advancedMenuItems: MenuItem[] = [
  {
    route: "/embeds",
    title: "Embeds",
    icon: "#embeds",
    permissions: null
  },
  {
    route: "/settings",
    title: "Settings",
    icon: "#settings",
    permissions: null
  }
];

@Injectable({
  providedIn: "root"
})
export class MenuItemsProvider {
  getRegularMenuItemsSection(): MenuItemGroup {
    return {
      name: "regular",
      items: this.getRegularMenuItems()
    };
  }
  getRegularMenuItems(): MenuItem[] {
    return regularMenuItems.slice();
  }
  getAdvancedMenuItemsSection(): MenuItemGroup {
    return {
      name: "advanced",
      items: this.getAdvancedMenuItems()
    };
  }
  getAdvancedMenuItems(): MenuItem[] {
    return advancedMenuItems.slice();
  }
}
