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

  startLogin(contact: string): Observable<{ token: string; contact: string }> {
    return this.http.post<{ token: string; contact: string }>(
      `${environment.apiBaseUrl}/api/auth/start`,
      { contact },
    );
  }

  completeLogin(token: string): Observable<StoredSession> {
    return this.http
      .post<{ bearerToken: string; user: UserDto }>(`${environment.apiBaseUrl}/api/auth/complete`, { token })
      .pipe(
        tap((res) => {
          const stored: StoredSession = { bearerToken: res.bearerToken, user: res.user };
          this.session.set(stored);
          localStorage.setItem(STORAGE_KEY, JSON.stringify(stored));
        }),
      );
  }

  logout(): void {
    this.session.set(null);
    localStorage.removeItem(STORAGE_KEY);
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
