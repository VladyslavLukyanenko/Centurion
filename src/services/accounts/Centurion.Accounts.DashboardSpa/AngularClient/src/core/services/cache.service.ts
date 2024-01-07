import {Injectable} from "@angular/core";

const cacheKey = "gg.centurion.dashboard:__GENERICCACHE__";

const getCacheKey = (key: string) => cacheKey + key;

@Injectable({
  providedIn: "root"
})
export class CacheService {
  private storage: StorageProvider = localStorage;

  getItem<T>(key: string): T | null {
    const rawItem = this.storage.getItem(getCacheKey(key));
    if (!rawItem) {
      return null;
    }

    const item: CacheEntry = JSON.parse(rawItem);
    if (!item) {
      return null;
    }

    if (!item.expirationDate || item.expirationDate > Date.now()) {
      return item.value;
    }

    this.storage.removeItem(getCacheKey(key));

    return null;
  }

  setItem(key: string, item: any, expirationDate?: Date): void {
    const entry = new CacheEntry(expirationDate ? expirationDate.valueOf() : null, item);

    this.storage.setItem(getCacheKey(key), JSON.stringify(entry));
  }
  removeItem(key: string): void {
    this.storage.removeItem(getCacheKey(key));
  }
}

class CacheEntry {
  constructor(readonly expirationDate: number | null, readonly value: any) {
  }
}

interface StorageProvider {
  setItem(key: string, value: string): void;
  removeItem(key: string): void;
  getItem(key: string): string | null;
}
