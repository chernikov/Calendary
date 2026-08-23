import { Component, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../core/auth.service';
import { OrderService } from '../../core/order.service';

@Component({
  selector: 'app-start',
  standalone: true,
  imports: [FormsModule],
  template: `
    <div class="page page-narrow">
      @if (stage() === 'enter') {
        <h2 style="font-size: 28px;">Почнімо</h2>
        <p class="text-muted">Без пароля — надішлемо посилання на пошту або телефон.</p>
        <div class="field" style="margin-top: var(--space-4);">
          <label for="contact">Пошта або телефон</label>
          <input
            id="contact"
            class="input"
            [(ngModel)]="contact"
            placeholder="you@example.com"
            (keyup.enter)="submitContact()"
          />
        </div>
        @if (error()) {
          <p style="color: var(--color-accent-2-700); font-size: 13px;">{{ error() }}</p>
        }
        <button class="btn btn-primary btn-block" [disabled]="!contact() || loading()" (click)="submitContact()">
          Надіслати посилання
        </button>
      }

      @if (stage() === 'sent') {
        <h2 style="font-size: 28px;">Посилання надіслано</h2>
        <p class="text-muted">
          Ми надіслали посилання на <strong>{{ contact() }}</strong>. У цьому демо немає реальної пошти —
          натисніть кнопку нижче, щоб відкрити його так, ніби ви перейшли за посиланням.
        </p>
        <button class="btn btn-primary btn-block" [disabled]="loading()" (click)="openLink()">
          Відкрити посилання
        </button>
      }
    </div>
  `,
})
export class StartComponent {
  readonly stage = signal<'enter' | 'sent'>('enter');
  readonly contact = signal('');
  readonly loading = signal(false);
  readonly error = signal<string | null>(null);
  private pendingToken = '';

  constructor(
    private readonly auth: AuthService,
    private readonly orders: OrderService,
    private readonly router: Router,
  ) {}

  submitContact(): void {
    if (!this.contact()) return;
    this.loading.set(true);
    this.error.set(null);
    this.auth.startLogin(this.contact()).subscribe({
      next: (res) => {
        this.pendingToken = res.token;
        this.stage.set('sent');
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Не вдалося надіслати посилання. Спробуйте ще раз.');
        this.loading.set(false);
      },
    });
  }

  openLink(): void {
    this.loading.set(true);
    this.auth.completeLogin(this.pendingToken).subscribe({
      next: () => {
        this.orders.createOrder().subscribe({
          next: (order) => this.router.navigate(['/order', order.id, 'upload']),
          error: () => {
            this.error.set('Не вдалося створити замовлення.');
            this.loading.set(false);
          },
        });
      },
      error: () => {
        this.error.set('Посилання недійсне.');
        this.loading.set(false);
        this.stage.set('enter');
      },
    });
  }
}
