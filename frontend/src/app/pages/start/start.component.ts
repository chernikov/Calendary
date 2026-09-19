import { AfterViewInit, Component, ElementRef, ViewChild, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { AuthService } from '../../core/auth.service';
import { OrderService } from '../../core/order.service';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'app-start',
  standalone: true,
  imports: [FormsModule],
  template: `
    <div class="page page-narrow">
      <h2 style="font-size: 28px;">{{ mode() === 'register' ? 'Реєстрація' : 'Вхід' }}</h2>

      <div class="field" style="margin-top: var(--space-4);">
        <label for="email">Пошта</label>
        <input id="email" class="input" type="email" [(ngModel)]="email" placeholder="you@example.com" />
      </div>

      @if (mode() === 'register') {
        <div class="field" style="margin-top: var(--space-3);">
          <label for="displayName">Ім'я</label>
          <input id="displayName" class="input" [(ngModel)]="displayName" />
        </div>
      }

      <div class="field" style="margin-top: var(--space-3);">
        <label for="password">Пароль</label>
        <input
          id="password"
          class="input"
          type="password"
          [(ngModel)]="password"
          (keyup.enter)="submit()"
          placeholder="мінімум 8 символів"
        />
      </div>

      @if (error()) {
        <p style="color: var(--color-accent-2-700); font-size: 13px; margin-top: var(--space-2);">{{ error() }}</p>
      }

      <button
        class="btn btn-primary btn-block"
        [disabled]="!email || !password || loading()"
        (click)="submit()"
      >
        {{ mode() === 'register' ? 'Зареєструватися' : 'Увійти' }}
      </button>

      <button class="btn btn-ghost btn-block" style="margin-top: var(--space-2);" (click)="toggleMode()">
        {{ mode() === 'register' ? 'Вже є акаунт? Увійти' : 'Немає акаунту? Зареєструватися' }}
      </button>

      <div style="display: flex; align-items: center; gap: 12px; color: var(--color-neutral-500); font-size: 11px; margin: var(--space-4) 0;">
        <span style="height: 1px; background: var(--color-divider); flex: 1;"></span>
        або
        <span style="height: 1px; background: var(--color-divider); flex: 1;"></span>
      </div>

      <div #googleButton></div>
    </div>
  `,
})
export class StartComponent implements AfterViewInit {
  @ViewChild('googleButton') googleButtonRef!: ElementRef<HTMLDivElement>;

  readonly mode = signal<'register' | 'login'>('register');
  readonly loading = signal(false);
  readonly error = signal<string | null>(null);

  email = '';
  password = '';
  displayName = '';

  constructor(
    private readonly auth: AuthService,
    private readonly orders: OrderService,
    private readonly router: Router,
    route: ActivatedRoute,
  ) {
    if (route.snapshot.queryParamMap.get('mode') === 'login') {
      this.mode.set('login');
    }
  }

  ngAfterViewInit(): void {
    if (!environment.googleClientId) return;

    // index.html loads the GSI script with async/defer, so it may well still be in flight when
    // this runs (Angular's view init is faster than the script fetch) — poll briefly instead of
    // giving up immediately.
    this.waitForGoogleIdentity(() => this.renderGoogleButton());
  }

  private waitForGoogleIdentity(onReady: () => void, attemptsLeft = 50): void {
    if (window.google?.accounts) {
      onReady();
      return;
    }
    if (attemptsLeft <= 0) return;
    setTimeout(() => this.waitForGoogleIdentity(onReady, attemptsLeft - 1), 100);
  }

  private renderGoogleButton(): void {
    window.google!.accounts.id.initialize({
      client_id: environment.googleClientId,
      callback: (response) => this.submitGoogleCredential(response.credential),
    });
    window.google!.accounts.id.renderButton(this.googleButtonRef.nativeElement, {
      theme: 'outline',
      size: 'large',
      width: 320,
    });
  }

  toggleMode(): void {
    this.mode.set(this.mode() === 'register' ? 'login' : 'register');
    this.error.set(null);
  }

  submit(): void {
    if (!this.email || !this.password) return;
    this.loading.set(true);
    this.error.set(null);

    const request$ =
      this.mode() === 'register'
        ? this.auth.register(this.email, this.password, this.displayName)
        : this.auth.login(this.email, this.password);

    request$.subscribe({
      next: () => this.afterAuthenticated(),
      error: (err) => {
        this.error.set(
          this.mode() === 'register'
            ? 'Цю пошту вже зареєстровано.'
            : err.status === 401
              ? 'Невірна пошта або пароль.'
              : 'Щось пішло не так. Спробуйте ще раз.',
        );
        this.loading.set(false);
      },
    });
  }

  private submitGoogleCredential(idToken: string): void {
    this.loading.set(true);
    this.error.set(null);
    this.auth.loginWithGoogle(idToken).subscribe({
      next: () => this.afterAuthenticated(),
      error: () => {
        this.error.set('Не вдалося увійти через Google.');
        this.loading.set(false);
      },
    });
  }

  private afterAuthenticated(): void {
    this.orders.createOrder().subscribe({
      next: (order) => this.router.navigate(['/order', order.id, 'upload']),
      error: () => {
        this.error.set('Не вдалося створити замовлення.');
        this.loading.set(false);
      },
    });
  }
}
