export interface Identity {
  id: number;
  email: string;
  discriminator: string;
  avatar: string;
  name: string;
  discordId: number;
  roleNames: string[];
  roleIds: number[];
  permissions: string[];
  isCurrentDashboardOwner: boolean;
  dashboard: string;
}
