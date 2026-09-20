import { Component, computed, signal } from '@angular/core';
import { NavigationEnd, Router, RouterLink, RouterLinkActive } from '@angular/router';
import { filter } from 'rxjs';
import { AuthService } from './core/auth.service';

@Component({
  selector: 'app-top-nav',
  standalone: true,
  imports: [RouterLink, RouterLinkActive],
  template: `
    @if (visible()) {
      <nav class="nav nav-top">
        <a class="nav-brand" routerLink="/">Calendary</a>

        @if (auth.isAuthenticated()) {
          <a routerLink="/orders" routerLinkActive="is-active" class="nav-icon-link" title="Кошик" style="display: inline-flex; align-items: center; gap: 5px;">
            <svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
              <circle cx="9" cy="21" r="1" />
              <circle cx="20" cy="21" r="1" />
              <path d="M1 1h4l2.68 13.39a2 2 0 0 0 2 1.61h9.72a2 2 0 0 0 2-1.61L23 6H6" />
            </svg>
            <span class="nav-label">Кошик</span>
          </a>
          <a routerLink="/my-orders" routerLinkActive="is-active" class="nav-icon-link" title="Мої замовлення" style="display: inline-flex; align-items: center; gap: 5px;">
            <svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
              <path d="M21 8a2 2 0 0 0-1-1.73l-7-4a2 2 0 0 0-2 0l-7 4A2 2 0 0 0 3 8v8a2 2 0 0 0 1 1.73l7 4a2 2 0 0 0 2 0l7-4a2 2 0 0 0 1-1.73Z" />
              <path d="M3.3 7 12 12l8.7-5M12 22V12" />
            </svg>
            <span class="nav-label">Мої замовлення</span>
          </a>
          @if (auth.user()?.role === 'Admin') {
            <a routerLink="/admin" class="nav-icon-link" title="Адмінка" style="display: inline-flex; align-items: center; gap: 5px;">
              <svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                <path d="M12 2 4 5v6c0 5 3.5 9 8 11 4.5-2 8-6 8-11V5z" />
              </svg>
              <span class="nav-label">Адмінка</span>
            </a>
          }
          <button class="btn btn-primary nav-icon-link" title="Створити календар" (click)="createOrder()" style="display: inline-flex; align-items: center; gap: 5px;">
            <svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
              <path d="M12 5v14M5 12h14" />
            </svg>
            <span class="nav-label">Створити календар</span>
          </button>
          <span class="text-muted nav-label" style="font-size: 13px;">{{ auth.user()?.displayName || auth.user()?.email }}</span>
          <button class="btn btn-ghost nav-icon-link" title="Вихід" (click)="logout()" style="display: inline-flex; align-items: center; gap: 5px;">
            <svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
              <path d="M9 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h4" />
              <path d="M16 17l5-5-5-5" />
              <path d="M21 12H9" />
            </svg>
            <span class="nav-label">Вихід</span>
          </button>
        } @else {
          <a routerLink="/start" [queryParams]="{ mode: 'login' }">Вхід</a>
          <a class="btn btn-primary" routerLink="/start" [queryParams]="{ mode: 'register' }">Зареєструватись</a>
        }
      </nav>
    }
  `,
})
export class TopNavComponent {
  private readonly url = signal('');

  // The admin area renders its own ng-zorro shell, so the customer-facing bar would be duplicate chrome there.
  readonly visible = computed(() => !this.url().startsWith('/admin'));

  constructor(
    readonly auth: AuthService,
    private readonly router: Router,
  ) {
    this.url.set(this.router.url);
    this.router.events
      .pipe(filter((e): e is NavigationEnd => e instanceof NavigationEnd))
      .subscribe((e) => this.url.set(e.urlAfterRedirects));
  }

  createOrder(): void {
    // The order itself isn't created until a photo is actually uploaded — see #348.
    this.router.navigate(['/order', 'new', 'upload']);
  }

  logout(): void {
    this.auth.logout();
    this.router.navigate(['/']);
  }
}
