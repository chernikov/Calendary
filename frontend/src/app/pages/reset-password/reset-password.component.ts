import { Component, OnInit, signal } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { AuthService } from '../../core/auth.service';

@Component({
    selector: 'app-reset-password',
    imports: [FormsModule, RouterLink],
    template: `
    <div class="page page-narrow">
      <h2 style="font-size: 28px;">Новий пароль</h2>

      @if (!token) {
        <p style="color: var(--color-accent-2-700); font-size: 13px; margin-top: var(--space-3);">
          Посилання недійсне — перейдіть за посиланням з листа ще раз.
        </p>
        <a class="btn btn-secondary" style="margin-top: var(--space-2);" routerLink="/forgot-password">Запросити нове</a>
      } @else if (done()) {
        <p class="text-muted" style="margin-top: var(--space-3);">Пароль оновлено. Тепер можете увійти з ним.</p>
        <a class="btn btn-primary" style="margin-top: var(--space-2);" routerLink="/start" [queryParams]="{ mode: 'login' }">
          Увійти
        </a>
      } @else {
        <div class="field" style="margin-top: var(--space-4);">
          <label for="password">Новий пароль</label>
          <input
            id="password"
            class="input"
            type="password"
            [(ngModel)]="password"
            placeholder="мінімум 8 символів"
          />
        </div>

        <div class="field" style="margin-top: var(--space-3);">
          <label for="confirm">Повторіть пароль</label>
          <input
            id="confirm"
            class="input"
            type="password"
            [(ngModel)]="confirmPassword"
            (keyup.enter)="submit()"
          />
        </div>

        @if (error()) {
          <p style="color: var(--color-accent-2-700); font-size: 13px; margin-top: var(--space-2);">{{ error() }}</p>
        }

        <button
          class="btn btn-primary btn-block"
          style="margin-top: var(--space-3);"
          [disabled]="!password || password !== confirmPassword || loading()"
          (click)="submit()"
        >
          Встановити пароль
        </button>
      }
    </div>
  `
})
export class ResetPasswordComponent implements OnInit {
  readonly loading = signal(false);
  readonly done = signal(false);
  readonly error = signal<string | null>(null);

  token = '';
  password = '';
  confirmPassword = '';

  constructor(
    private readonly route: ActivatedRoute,
    private readonly auth: AuthService,
  ) {}

  ngOnInit(): void {
    this.token = this.route.snapshot.queryParamMap.get('token') ?? '';
  }

  submit(): void {
    if (!this.password || this.password !== this.confirmPassword) return;
    this.loading.set(true);
    this.error.set(null);

    this.auth.resetPassword(this.token, this.password).subscribe({
      next: () => {
        this.loading.set(false);
        this.done.set(true);
      },
      error: (err: HttpErrorResponse) => {
        this.loading.set(false);
        this.error.set(typeof err.error === 'string' ? err.error : 'Щось пішло не так. Спробуйте ще раз.');
      },
    });
  }
}
