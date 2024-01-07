type CtorFunction = (ctor: any) => void;

export const PERMISSIONS_HOLDER_KEY = "__permissions__";

export function AuthorizePermissions(...names: string[]): CtorFunction {
  return (ctor: () => void) => {
    Object.defineProperty(ctor, PERMISSIONS_HOLDER_KEY, {
      value: names,
      writable: false
    });
  };
}

export const extractDeclaredPermissions = (targetCtor: any): string[] => {
  if (!(PERMISSIONS_HOLDER_KEY in targetCtor)) {
    return [];
  }

  return targetCtor[PERMISSIONS_HOLDER_KEY];
};
