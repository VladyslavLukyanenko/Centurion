import {Injectable} from "@angular/core";

const cacheKey = "monitors-panel:__GENERICCACHE__";

const getCacheKey = (key: string) => cacheKey + key;

@Injectable({
  providedIn: "root"
})
export class CacheService {
  private storage: StorageProvider = localStorage;

  getItem<T>(key: string): T {
    const rawItem = this.storage.getItem(getCacheKey(key));
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
  constructor(readonly expirationDate: number, readonly value: any) {
  }
}

interface StorageProvider {
  setItem(key: string, value: string): void;
  removeItem(key: string): void;
  getItem(key: string): string;
}
