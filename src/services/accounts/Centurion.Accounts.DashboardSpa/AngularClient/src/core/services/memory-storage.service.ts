import {Injectable} from "@angular/core";

@Injectable({
  providedIn: "root"
})

export class MemoryStorageService {
  private storage: { [key: string]: any } = {};

  setItem(key: string, value: any): void {
    this.storage[key] = value;
  }

  getItem(key: string): any {
    return this.storage[key];
  }

  getObject<T>(key: string): T {
    return this.contains(key) ? JSON.parse(this.getItem(key)) : null;
  }

  extractObject<T>(key: string): T {
    return this.contains(key) ? JSON.parse(this.extract(key)) : null;
  }

  extract(key: string): string {
    const value = this.getItem(key);
    this.removeItem(key);
    return value;
  }

  removeItem(key: string): void {
    delete this.storage[key];
  }

  contains(key: string): boolean {
    return key in this.storage;
  }

  clear(): void {
    for (const storageKey in this.storage) {
      if (this.storage.hasOwnProperty(storageKey)) {
        delete this.storage[storageKey];
      }
    }
  }
}
