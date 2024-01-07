export namespace Menu {
  export class MenuItem<T> {
    type?: T;
    name: string;
    translationPath: string;
    iconName?: string;
    children?: Array<MenuItem<T>>;

    constructor(name: string, translationPath: string, type?: T | any, iconName?: string, children?: Array<MenuItem<T>>) {
      this.type = type;
      this.name = name;
      this.translationPath = translationPath;
      this.iconName = iconName;
      this.children = children;
    }
  }
}
