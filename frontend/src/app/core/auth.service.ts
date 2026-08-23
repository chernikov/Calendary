import { Injectable, computed, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { UserDto } from './models';
import { environment } from '../../environments/environment';

const STORAGE_KEY = 'calendary.session';

interface StoredSession {
  bearerToken: string;
  user: UserDto;
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly session = signal<StoredSession | null>(this.readStoredSession());

  readonly user = computed(() => this.session()?.user ?? null);
  readonly isAuthenticated = computed(() => this.session() !== null);

  constructor(private readonly http: HttpClient) {}

  get bearerToken(): string | null {
    return this.session()?.bearerToken ?? null;
  }

  register(email: string, password: string, displayName: string): Observable<StoredSession> {
    return this.http
      .post<{ bearerToken: string; user: UserDto }>(`${environment.apiBaseUrl}/api/auth/register`, {
        email,
        password,
        displayName,
      })
      .pipe(tap((res) => this.storeSession(res)));
  }

  login(email: string, password: string): Observable<StoredSession> {
    return this.http
      .post<{ bearerToken: string; user: UserDto }>(`${environment.apiBaseUrl}/api/auth/login`, {
        email,
        password,
      })
      .pipe(tap((res) => this.storeSession(res)));
  }

  loginWithGoogle(idToken: string): Observable<StoredSession> {
    return this.http
      .post<{ bearerToken: string; user: UserDto }>(`${environment.apiBaseUrl}/api/auth/google`, {
        idToken,
      })
      .pipe(tap((res) => this.storeSession(res)));
  }

  logout(): void {
    this.session.set(null);
    localStorage.removeItem(STORAGE_KEY);
  }

  private storeSession(res: { bearerToken: string; user: UserDto }): void {
    const stored: StoredSession = { bearerToken: res.bearerToken, user: res.user };
    this.session.set(stored);
    localStorage.setItem(STORAGE_KEY, JSON.stringify(stored));
  }

  private readStoredSession(): StoredSession | null {
    const raw = localStorage.getItem(STORAGE_KEY);
    if (!raw) return null;
    try {
      return JSON.parse(raw) as StoredSession;
    } catch {
      return null;
    }
  }
}
