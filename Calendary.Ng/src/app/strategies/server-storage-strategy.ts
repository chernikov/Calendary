import { StorageStrategy } from './storage-strategy';

export class ServerStorageStrategy implements StorageStrategy {
  private token: string | null = null;

  saveToken(token: string): void {
    this.token = token;
  }

  getToken(): string | null {
    return this.token;
  }

  removeToken(): void {
    this.token = null;
  }
}