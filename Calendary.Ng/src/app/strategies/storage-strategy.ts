export interface StorageStrategy {
    saveToken(token: string): void;
    getToken(): string | null;
    removeToken(): void;
  }