import { Component, computed, signal } from '@angular/core';
import { NavigationEnd, Router, RouterLink } from '@angular/router';
import { filter } from 'rxjs';
import { AuthService } from './core/auth.service';

@Component({
  selector: 'app-top-nav',
  standalone: true,
  imports: [RouterLink],
  template: `
    @if (visible()) {
      <nav class="nav nav-top">
        <a class="nav-brand" routerLink="/">Calendary</a>

        @if (auth.isAuthenticated()) {
          <span class="text-muted" style="font-size: 13px;">{{ auth.user()?.displayName || auth.user()?.email }}</span>
          <button class="btn btn-ghost" (click)="logout()">Вихід</button>
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

  logout(): void {
    this.auth.logout();
    this.router.navigate(['/']);
  }
}
