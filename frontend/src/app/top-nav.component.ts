import { Component, computed, signal } from '@angular/core';
import { NavigationEnd, Router, RouterLink, RouterLinkActive } from '@angular/router';
import { filter } from 'rxjs';
import { AuthService } from './core/auth.service';
import { OrderService } from './core/order.service';

@Component({
  selector: 'app-top-nav',
  standalone: true,
  imports: [RouterLink, RouterLinkActive],
  template: `
    @if (visible()) {
      <nav class="nav nav-top">
        <a class="nav-brand" routerLink="/">Calendary</a>

        @if (auth.isAuthenticated()) {
          <a routerLink="/orders" routerLinkActive="is-active">Мої замовлення</a>
          @if (auth.user()?.role === 'Admin') {
            <a routerLink="/admin">Адмінка</a>
          }
          <button class="btn btn-primary" [disabled]="creating()" (click)="createOrder()">Створити календар</button>
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
  readonly creating = signal(false);

  // The admin area renders its own ng-zorro shell, so the customer-facing bar would be duplicate chrome there.
  readonly visible = computed(() => !this.url().startsWith('/admin'));

  constructor(
    readonly auth: AuthService,
    private readonly router: Router,
    private readonly orders: OrderService,
  ) {
    this.url.set(this.router.url);
    this.router.events
      .pipe(filter((e): e is NavigationEnd => e instanceof NavigationEnd))
      .subscribe((e) => this.url.set(e.urlAfterRedirects));
  }

  createOrder(): void {
    this.creating.set(true);
    this.orders.createOrder().subscribe({
      next: (order) => {
        this.creating.set(false);
        this.router.navigate(['/order', order.id, 'upload']);
      },
      error: () => this.creating.set(false),
    });
  }

  logout(): void {
    this.auth.logout();
    this.router.navigate(['/']);
  }
}
