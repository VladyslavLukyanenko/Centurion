export interface Identity {
  id: number;
  email: string;
  isAdmin: boolean;
  roles: Array<string>;
  avatar: string;
  firstName: string;
  lastName: string;
}
