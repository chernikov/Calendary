import { Injectable, Inject, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { StorageStrategy } from '../app/strategies/storage-strategy';
import { BrowserStorageStrategy } from '../app/strategies/browser-storage-strategy';
import { ServerStorageStrategy } from '../app/strategies/server-storage-strategy';

@Injectable({
  providedIn: 'root',
})
export class TokenService {
  private tokenKey = 'jwtToken';

  private storageStrategy: StorageStrategy;
  constructor(@Inject(PLATFORM_ID) private platformId: object) {
    // Вибір стратегії залежно від платформи
    if (isPlatformBrowser(this.platformId)) {
      this.storageStrategy = new BrowserStorageStrategy();
    } else {
      this.storageStrategy = new ServerStorageStrategy();
    }
  }

  saveToken(token: string): void {
    this.storageStrategy.saveToken(token);
  }

  getToken(): string | null {
    return this.storageStrategy.getToken();
  }

  removeToken(): void {
    this.storageStrategy.removeToken();
  }
  
  // Перевіряє наявність токену
  hasToken(): boolean {
    return this.getToken() !== null;
  }
}
