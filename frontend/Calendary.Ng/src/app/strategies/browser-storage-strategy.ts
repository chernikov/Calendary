import { StorageStrategy } from './storage-strategy';

export class BrowserStorageStrategy implements StorageStrategy {
  private tokenKey = 'jwtToken';

  saveToken(token: string): void {
    localStorage.setItem(this.tokenKey, token);
  }

  getToken(): string | null {
    return localStorage.getItem(this.tokenKey);
  }

  removeToken(): void {
    localStorage.removeItem(this.tokenKey);
  }
}