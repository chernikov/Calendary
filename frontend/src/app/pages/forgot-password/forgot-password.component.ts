import { Component, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../core/auth.service';

@Component({
    selector: 'app-forgot-password',
    imports: [FormsModule, RouterLink],
    template: `
    <div class="page page-narrow">
      <h2 style="font-size: 28px;">Відновлення пароля</h2>

      @if (sent()) {
        <p class="text-muted" style="margin-top: var(--space-3);">
          Якщо на цю пошту зареєстровано акаунт з паролем, ми надіслали лист з посиланням для встановлення нового
          пароля. Перевірте пошту — лист дійсний 1 годину.
        </p>
        <a class="btn btn-secondary" style="margin-top: var(--space-3);" routerLink="/start" [queryParams]="{ mode: 'login' }">
          До входу
        </a>
      } @else {
        <p class="text-muted">Вкажіть пошту, і ми надішлемо посилання для встановлення нового пароля.</p>

        <div class="field" style="margin-top: var(--space-4);">
          <label for="email">Пошта</label>
          <input id="email" class="input" type="email" [(ngModel)]="email" (keyup.enter)="submit()" placeholder="you@example.com" />
        </div>

        @if (error()) {
          <p style="color: var(--color-accent-2-700); font-size: 13px; margin-top: var(--space-2);">{{ error() }}</p>
        }

        <button class="btn btn-primary btn-block" style="margin-top: var(--space-3);" [disabled]="!email || loading()" (click)="submit()">
          Надіслати посилання
        </button>

        <a class="btn btn-ghost btn-block" style="margin-top: var(--space-2);" routerLink="/start" [queryParams]="{ mode: 'login' }">
          Назад до входу
        </a>
      }
    </div>
  `
})
export class ForgotPasswordComponent {
  readonly loading = signal(false);
  readonly sent = signal(false);
  readonly error = signal<string | null>(null);

  email = '';

  constructor(private readonly auth: AuthService) {}

  submit(): void {
    if (!this.email) return;
    this.loading.set(true);
    this.error.set(null);

    this.auth.forgotPassword(this.email).subscribe({
      // The backend always returns 200 regardless of whether the email exists — the UI shows the
      // same "check your email" message either way, so this can't be used to enumerate accounts.
      next: () => {
        this.loading.set(false);
        this.sent.set(true);
      },
      error: () => {
        this.loading.set(false);
        this.error.set('Щось пішло не так. Спробуйте ще раз.');
      },
    });
  }
}
