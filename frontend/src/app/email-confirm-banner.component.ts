import { Component, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AuthService } from './core/auth.service';

@Component({
  selector: 'app-email-confirm-banner',
  standalone: true,
  imports: [FormsModule],
  template: `
    @if (auth.needsEmailConfirmation()) {
      <div class="banner-top">
        <span>Підтвердіть пошту {{ auth.user()?.email }}, щоб не втратити доступ до замовлення.</span>
        <button class="btn btn-ghost" (click)="open()">Підтвердити</button>
      </div>
    }

    @if (auth.confirmModalOpen()) {
      <div class="dialog-backdrop" (click)="close()">
        <div class="dialog" (click)="$event.stopPropagation()">
          <div class="dialog-title">Підтвердження пошти</div>
          <p class="dialog-body">
            Ми надіслали 6-значний код на {{ auth.user()?.email }}. Введіть його нижче.
          </p>

          <div class="field">
            <label>Код підтвердження</label>
            <input
              class="input"
              style="letter-spacing: 6px; font-size: 18px; text-align: center;"
              maxlength="6"
              inputmode="numeric"
              [(ngModel)]="code"
              (keyup.enter)="submit()"
            />
          </div>

          @if (error()) {
            <p style="color: var(--color-accent-2-700); font-size: 13px;">{{ error() }}</p>
          }
          @if (resendMessage()) {
            <p class="text-muted" style="font-size: 12px;">{{ resendMessage() }}</p>
          }

          <div class="dialog-actions" style="justify-content: space-between;">
            <button class="btn btn-ghost" [disabled]="resendLoading()" (click)="resend()">
              Надіслати код ще раз
            </button>
            <div style="display: flex; gap: var(--space-2);">
              <button class="btn btn-secondary" (click)="close()">Скасувати</button>
              <button class="btn btn-primary" [disabled]="code.length !== 6 || loading()" (click)="submit()">
                Підтвердити
              </button>
            </div>
          </div>
        </div>
      </div>
    }
  `,
})
export class EmailConfirmBannerComponent {
  readonly loading = signal(false);
  readonly error = signal<string | null>(null);
  readonly resendLoading = signal(false);
  readonly resendMessage = signal<string | null>(null);

  code = '';

  constructor(readonly auth: AuthService) {}

  open(): void {
    this.code = '';
    this.error.set(null);
    this.resendMessage.set(null);
    this.auth.openConfirmModal();
  }

  close(): void {
    this.auth.closeConfirmModal();
  }

  submit(): void {
    if (this.code.length !== 6) return;
    this.loading.set(true);
    this.error.set(null);
    this.auth.confirmEmail(this.code).subscribe({
      next: () => {
        this.loading.set(false);
        this.close();
      },
      error: () => {
        this.loading.set(false);
        this.error.set('Невірний або прострочений код.');
      },
    });
  }

  resend(): void {
    this.resendLoading.set(true);
    this.resendMessage.set(null);
    this.auth.resendConfirmation().subscribe({
      next: () => {
        this.resendLoading.set(false);
        this.resendMessage.set('Код надіслано ще раз.');
      },
      error: () => {
        this.resendLoading.set(false);
        this.resendMessage.set('Не вдалося надіслати код. Спробуйте пізніше.');
      },
    });
  }
}
