export interface ActiveFilter {
  displayName: string;
  label: string;
  value: any;
  kind: string;
  command: () => Promise<any> | any,
}
