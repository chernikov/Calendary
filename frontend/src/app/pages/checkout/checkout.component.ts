import { Component, OnInit, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { Store } from '@ngrx/store';
import { Actions, ofType } from '@ngrx/effects';
import {
  OrderActions,
  selectOrder,
  selectOrderBusy,
  selectOrderError,
  selectWarehouses,
} from '../../core/state/order';
import { AuthService } from '../../core/auth.service';
import { NovaPoshtaWarehouseDto } from '../../core/models';

@Component({
  selector: 'app-checkout',
  standalone: true,
  imports: [FormsModule],
  template: `
    <div class="page page-narrow">
      @if (order(); as o) {
        <h2 style="font-size: 28px;">Куди доставити</h2>
        <p class="text-muted">Доставка Новою поштою входить у ціну. Надсилаємо у твердому тубусі.</p>

        <div style="display: flex; flex-direction: column; gap: 14px; margin: var(--space-3) 0;">
          <div class="field">
            <label>Отримувач</label>
            <input class="input" [(ngModel)]="recipientName" />
          </div>
          <div class="field">
            <label>Телефон</label>
            <input class="input" [(ngModel)]="phone" placeholder="+380 67 000 00 00" />
          </div>
          <div class="field">
            <label>Місто</label>
            <input class="input" [(ngModel)]="city" (ngModelChange)="onCityChange($event)" />
          </div>

          @if (warehouses().length > 0) {
            <div>
              <div class="text-muted" style="font-size: 11px; margin-bottom: 5px;">Відділення</div>
              <div style="border: 1px solid var(--color-divider);">
                @for (w of warehouses(); track w.number) {
                  <div
                    style="display: flex; gap: 10px; padding: 11px; border-bottom: 1px solid var(--color-divider); cursor: pointer;"
                    [style.background]="selectedWarehouse()?.number === w.number ? 'var(--color-accent-100)' : 'transparent'"
                    (click)="selectedWarehouse.set(w)"
                  >
                    <span class="money" style="font-size: 11.5px; color: var(--color-accent-700); width: 30px; flex: none;">{{ w.number }}</span>
                    <span style="font-size: 12.5px;">{{ w.address }} · {{ w.closesAt }}</span>
                  </div>
                }
              </div>
            </div>
          }
        </div>

        <div class="hr"></div>

        <h2 style="font-size: 28px;">Оплата</h2>
        <div style="display: flex; flex-direction: column; gap: 10px;">
          @for (m of paymentMethods; track m.value) {
            <button
              class="btn"
              [class.btn-primary]="method() === m.value"
              [class.btn-secondary]="method() !== m.value"
              style="min-height: 50px; font-size: 15px; justify-content: space-between; padding-inline: 16px;"
              (click)="method.set(m.value)"
            >
              <span>{{ m.label }}</span>
            </button>
          }
        </div>

        @if (error()) {
          <p style="color: var(--color-accent-2-700); font-size: 13px; margin-top: var(--space-2);">{{ error() }}</p>
        }

        @if (auth.needsEmailConfirmation()) {
          <p class="text-muted" style="font-size: 12px; margin-top: var(--space-2);">
            Пошту не підтверджено — ви можете не отримати сповіщення про статус замовлення.
            <button class="btn btn-ghost" style="padding: 0;" (click)="auth.openConfirmModal()">Підтвердити</button>
          </p>
        }

        <button
          class="btn btn-primary btn-block"
          style="min-height: 50px; font-size: 15px;"
          [disabled]="!canSubmit() || busy()"
          (click)="submit()"
        >
          Оплатити {{ o.price }} ₴
        </button>
        <p class="text-muted" style="font-size: 11px; text-align: center; margin-top: 8px;">
          Друк починається одразу після оплати.
        </p>
      }
    </div>
  `,
})
export class CheckoutComponent implements OnInit {
  private readonly store = inject(Store);
  readonly order = this.store.selectSignal(selectOrder);
  readonly warehouses = this.store.selectSignal(selectWarehouses);
  readonly selectedWarehouse = signal<NovaPoshtaWarehouseDto | null>(null);
  readonly method = signal<string>('ApplePay');
  readonly busy = this.store.selectSignal(selectOrderBusy);
  readonly error = this.store.selectSignal(selectOrderError);

  readonly paymentMethods = [
    { value: 'ApplePay', label: 'Apple Pay' },
    { value: 'GooglePay', label: 'Google Pay' },
    { value: 'Monobank', label: 'Оплатити з monobank' },
    { value: 'Card', label: 'Карткою' },
  ];

  recipientName = '';
  phone = '';
  city = '';

  private readonly orderId: string;
  private cityDebounce?: ReturnType<typeof setTimeout>;

  constructor(
    private readonly route: ActivatedRoute,
    private readonly router: Router,
    private readonly actions$: Actions,
    readonly auth: AuthService,
  ) {
    this.orderId = this.route.snapshot.paramMap.get('orderId')!;
    this.actions$
      .pipe(ofType(OrderActions.checkoutAndPaySuccess), takeUntilDestroyed())
      .subscribe(() => this.router.navigate(['/order', this.orderId, 'status']));
  }

  ngOnInit(): void {
    this.store.dispatch(OrderActions.loadOrder({ orderId: this.orderId }));
  }

  onCityChange(city: string): void {
    this.selectedWarehouse.set(null);
    this.store.dispatch(OrderActions.clearWarehouses());
    clearTimeout(this.cityDebounce);
    if (!city.trim()) return;
    this.cityDebounce = setTimeout(() => {
      this.store.dispatch(OrderActions.loadWarehouses({ city: city.trim() }));
    }, 300);
  }

  canSubmit(): boolean {
    return !!(this.recipientName && this.phone && this.city && this.selectedWarehouse() && this.method());
  }

  submit(): void {
    const w = this.selectedWarehouse();
    if (!w) return;
    this.store.dispatch(
      OrderActions.checkoutAndPay({
        orderId: this.orderId,
        delivery: {
          recipientName: this.recipientName,
          phone: this.phone,
          city: this.city,
          warehouseNumber: w.number,
          warehouseAddress: w.address,
        },
        method: this.method(),
      }),
    );
  }
}
