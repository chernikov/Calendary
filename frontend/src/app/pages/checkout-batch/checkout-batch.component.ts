import { Component, OnInit, computed, effect, inject, signal } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { Store } from '@ngrx/store';
import {
  OrderActions,
  selectCities,
  selectMyOrders,
  selectOrderBusy,
  selectOrderError,
  selectWarehouses,
} from '../../core/state/order';
import { AuthService } from '../../core/auth.service';
import { OrderService } from '../../core/order.service';
import { NovaPoshtaWarehouseDto } from '../../core/models';

// Mirrors the backend's tolerant UkrainianPhoneNumber.Normalize (#304) — accepts the common ways
// a customer might type the number and normalizes to "+380XXXXXXXXX", or null if it doesn't fit.
function normalizeUaPhone(raw: string): string | null {
  const digits = raw.replace(/\D/g, '');
  let normalized: string | null = null;
  if (digits.length === 12 && digits.startsWith('380')) normalized = '+' + digits;
  else if (digits.length === 10 && digits.startsWith('0')) normalized = '+380' + digits.slice(1);
  else if (digits.length === 9) normalized = '+380' + digits;
  return normalized && /^\+380\d{9}$/.test(normalized) ? normalized : null;
}

@Component({
    selector: 'app-checkout-batch',
    imports: [FormsModule, RouterLink],
    template: `
    <div class="page page-narrow">
      <a routerLink="/orders" style="display: inline-flex; align-items: center; gap: 4px; margin-bottom: var(--space-2); font-size: 13.5px;">
        ← Кошик
      </a>

      <h2 style="font-size: 28px;">Оформлення кількох календарів</h2>

      @if (selectedOrders().length === 0) {
        <p class="text-muted">Замовлення не обрано.</p>
      } @else {
        <div style="display: flex; flex-direction: column; gap: var(--space-2); margin: var(--space-3) 0;">
          @for (o of selectedOrders(); track o.id) {
            <div style="display: flex; justify-content: space-between; align-items: center; font-size: 13.5px;">
              <span>{{ o.styleName || 'Без стилю' }} × {{ o.printQuantity }}</span>
              <span class="money">{{ o.price * o.printQuantity }} ₴</span>
            </div>
          }
        </div>

        <div class="hr"></div>

        <div style="display: flex; justify-content: space-between; align-items: baseline; padding: 16px 0;">
          <span class="d" style="font-family: var(--font-heading); font-weight: 600; font-size: 17px;">Разом</span>
          <span class="money" style="font-size: 30px; font-weight: 500;">{{ totalPrice() }} ₴</span>
        </div>

        <h2 style="font-size: 28px;">Куди доставити</h2>
        <p class="text-muted">Доставка Новою поштою входить у ціну. Усі обрані календарі відправляються разом.</p>

        <div style="display: flex; flex-direction: column; gap: 14px; margin: var(--space-3) 0;">
          <div class="field">
            <label>Отримувач</label>
            <input class="input" [(ngModel)]="recipientName" />
          </div>
          <div class="field">
            <label>Телефон</label>
            <div style="display: flex; gap: 8px;">
              <input class="input" style="flex: 1;" [(ngModel)]="phone" (ngModelChange)="onPhoneChange()" placeholder="+380 67 000 00 00" />
              @if (isPhoneValid() && !isPhoneVerified()) {
                <button
                  type="button"
                  class="btn btn-secondary"
                  style="white-space: nowrap;"
                  [disabled]="sendingCode()"
                  (click)="sendPhoneCode()"
                >
                  {{ codeSent() ? 'Надіслати ще раз' : 'Підтвердити' }}
                </button>
              }
            </div>
            @if (phone && !isPhoneValid()) {
              <p style="color: var(--color-accent-2-700); font-size: 12px; margin: 4px 0 0;">
                Невірний формат. Приклад: +380671234567.
              </p>
            }
            @if (isPhoneVerified()) {
              <p style="color: var(--color-accent-700); font-size: 12px; margin: 4px 0 0;">✓ Телефон підтверджено</p>
            }
            @if (codeSent() && !isPhoneVerified()) {
              <div style="display: flex; gap: 8px; margin-top: 8px;">
                <input
                  class="input"
                  style="flex: 1;"
                  [(ngModel)]="phoneCode"
                  placeholder="Код з SMS"
                  (keyup.enter)="confirmPhoneCode()"
                />
                <button type="button" class="btn btn-primary" [disabled]="!phoneCode || confirmingCode()" (click)="confirmPhoneCode()">
                  Підтвердити код
                </button>
              </div>
            }
            @if (phoneVerifyError()) {
              <p style="color: var(--color-accent-2-700); font-size: 12px; margin: 4px 0 0;">{{ phoneVerifyError() }}</p>
            }
          </div>
          <div class="field" style="position: relative;">
            <label>Місто</label>
            <input class="input" [(ngModel)]="city" (ngModelChange)="onCityChange($event)" autocomplete="off" />
            @if (showCitySuggestions() && cities().length > 0) {
              <div style="position: absolute; top: 100%; left: 0; right: 0; z-index: 10; margin-top: 2px; max-height: 220px; overflow-y: auto; background: var(--color-surface); border: 1px solid var(--color-divider); border-radius: var(--radius-sm); box-shadow: var(--shadow-md);">
                @for (c of cities(); track c) {
                  <div
                    style="padding: 8px 11px; font-size: 13px; cursor: pointer; border-bottom: 1px solid var(--color-divider);"
                    (click)="pickCity(c)"
                  >
                    {{ c }}
                  </div>
                }
              </div>
            }
          </div>

          @if (warehouses().length > 0) {
            <div class="field">
              <label>Відділення</label>
              <details class="select-dropdown" #warehouseDetails>
                <summary>{{ selectedWarehouse() ? (selectedWarehouse()!.number + ' · ' + selectedWarehouse()!.address) : 'Оберіть відділення' }}</summary>
                <div style="border-top: 1px solid var(--color-divider);">
                  <div style="display: flex; gap: 4px; padding: 8px;">
                    @for (f of warehouseFilters; track f.value) {
                      <button
                        type="button"
                        class="btn"
                        [class.btn-primary]="warehouseFilter() === f.value"
                        [class.btn-secondary]="warehouseFilter() !== f.value"
                        style="font-size: 11px; padding: 3px 9px; min-height: unset;"
                        (click)="warehouseFilter.set(f.value)"
                      >
                        {{ f.label }}
                      </button>
                    }
                  </div>
                  <div style="max-height: 280px; overflow-y: auto;">
                    @for (w of filteredWarehouses(); track w.number) {
                      <div
                        style="display: flex; gap: 10px; padding: 11px; border-top: 1px solid var(--color-divider); cursor: pointer;"
                        [style.background]="selectedWarehouse()?.number === w.number ? 'var(--color-accent-100)' : 'transparent'"
                        (click)="pickWarehouse(w, warehouseDetails)"
                      >
                        <span class="money" style="font-size: 11.5px; color: var(--color-accent-700); width: 30px; flex: none;">{{ w.number }}</span>
                        <span style="font-size: 12.5px; flex: 1;">
                          {{ w.address }} · {{ w.closesAt }}
                          @if (w.isPostomat) {
                            <span class="tag tag-neutral" style="font-size: 10px; margin-left: 6px;">Поштомат</span>
                          }
                        </span>
                      </div>
                    } @empty {
                      <div class="text-muted" style="padding: 11px; font-size: 12.5px;">Нічого не знайдено для цього фільтра.</div>
                    }
                  </div>
                </div>
              </details>
            </div>
          }
        </div>

        <div class="hr"></div>

        <h2 style="font-size: 28px;">Оплата</h2>
        <p class="text-muted" style="font-size: 13px;">Оплата карткою через monobank — після натискання ви перейдете на сторінку оплати monobank.</p>

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
          Оплатити {{ totalPrice() }} ₴ через monobank
        </button>
        <p class="text-muted" style="font-size: 11px; text-align: center; margin-top: 8px;">
          Друк починається одразу після оплати.
        </p>
      }
    </div>
  `
})
export class CheckoutBatchComponent implements OnInit {
  private readonly store = inject(Store);
  private readonly route = inject(ActivatedRoute);
  readonly auth = inject(AuthService);

  private readonly orderIds = signal<string[]>([]);
  readonly selectedOrders = computed(() => {
    const ids = new Set(this.orderIds());
    return this.store.selectSignal(selectMyOrders)().filter((o) => ids.has(o.id));
  });
  readonly totalPrice = computed(() => this.selectedOrders().reduce((sum, o) => sum + o.price * o.printQuantity, 0));

  readonly cities = this.store.selectSignal(selectCities);
  readonly warehouses = this.store.selectSignal(selectWarehouses);
  readonly showCitySuggestions = signal(false);
  readonly selectedWarehouse = signal<NovaPoshtaWarehouseDto | null>(null);
  readonly warehouseFilter = signal<'all' | 'branch' | 'postomat'>('all');
  readonly warehouseFilters: { value: 'all' | 'branch' | 'postomat'; label: string }[] = [
    { value: 'all', label: 'Усі' },
    { value: 'branch', label: 'Відділення' },
    { value: 'postomat', label: 'Поштомати' },
  ];
  readonly filteredWarehouses = computed(() => {
    const filter = this.warehouseFilter();
    const list = this.warehouses();
    if (filter === 'branch') return list.filter((w) => !w.isPostomat);
    if (filter === 'postomat') return list.filter((w) => w.isPostomat);
    return list;
  });
  readonly busy = this.store.selectSignal(selectOrderBusy);
  readonly error = this.store.selectSignal(selectOrderError);

  recipientName = '';
  phone = '';
  city = '';

  // #304 phone verification — local component state, same as CheckoutComponent.
  readonly codeSent = signal(false);
  readonly sendingCode = signal(false);
  readonly confirmingCode = signal(false);
  readonly phoneVerifyError = signal<string | null>(null);
  phoneCode = '';
  private verifiedPhoneValue: string | null = null;

  private readonly orders = inject(OrderService);
  private cityDebounce?: ReturnType<typeof setTimeout>;
  private prefilled = false;

  constructor() {
    // Prefills from the user's last-used checkout delivery info (#304) — unlike the single-order
    // checkout, there's no per-order Delivery to prefer here (a batch spans several orders).
    effect(() => {
      const source = this.auth.user()?.lastDelivery;
      if (!source || this.prefilled) return;
      this.prefilled = true;

      this.recipientName = source.recipientName;
      this.phone = source.phone;
      this.city = source.city;
      this.selectedWarehouse.set({
        number: source.warehouseNumber,
        address: source.warehouseAddress,
        closesAt: '',
        isPostomat: false,
      });
      this.store.dispatch(OrderActions.loadWarehouses({ city: source.city }));

      const normalized = normalizeUaPhone(source.phone);
      if (normalized && normalized === this.auth.user()?.verifiedPhone) {
        this.verifiedPhoneValue = normalized;
      }
    });
  }

  ngOnInit(): void {
    const raw = this.route.snapshot.queryParamMap.get('ids') ?? '';
    this.orderIds.set(raw.split(',').filter(Boolean));
    this.store.dispatch(OrderActions.loadMyOrders());
  }

  isPhoneValid(): boolean {
    return normalizeUaPhone(this.phone) !== null;
  }

  isPhoneVerified(): boolean {
    const normalized = normalizeUaPhone(this.phone);
    return normalized !== null && normalized === this.verifiedPhoneValue;
  }

  onPhoneChange(): void {
    this.codeSent.set(false);
    this.phoneCode = '';
    this.phoneVerifyError.set(null);
  }

  sendPhoneCode(): void {
    const phone = normalizeUaPhone(this.phone);
    if (!phone) return;
    this.sendingCode.set(true);
    this.phoneVerifyError.set(null);
    this.orders.sendPhoneVerification(phone).subscribe({
      next: () => {
        this.sendingCode.set(false);
        this.codeSent.set(true);
      },
      error: (err: HttpErrorResponse) => {
        this.sendingCode.set(false);
        this.phoneVerifyError.set(
          typeof err.error === 'string' ? err.error : 'Не вдалося надіслати код. Спробуйте ще раз.',
        );
      },
    });
  }

  confirmPhoneCode(): void {
    if (!this.phoneCode) return;
    const phone = normalizeUaPhone(this.phone);
    this.confirmingCode.set(true);
    this.phoneVerifyError.set(null);
    this.orders.confirmPhoneVerification(this.phoneCode).subscribe({
      next: () => {
        this.confirmingCode.set(false);
        this.codeSent.set(false);
        this.phoneCode = '';
        this.verifiedPhoneValue = phone;
      },
      error: (err: HttpErrorResponse) => {
        this.confirmingCode.set(false);
        this.phoneVerifyError.set(typeof err.error === 'string' ? err.error : 'Невірний код.');
      },
    });
  }

  onCityChange(city: string): void {
    this.selectedWarehouse.set(null);
    this.warehouseFilter.set('all');
    this.store.dispatch(OrderActions.clearWarehouses());
    this.showCitySuggestions.set(true);
    clearTimeout(this.cityDebounce);
    if (!city.trim()) {
      this.store.dispatch(OrderActions.clearCities());
      return;
    }
    this.cityDebounce = setTimeout(() => {
      this.store.dispatch(OrderActions.loadCities({ query: city.trim() }));
    }, 300);
  }

  pickCity(city: string): void {
    this.city = city;
    this.showCitySuggestions.set(false);
    this.store.dispatch(OrderActions.clearCities());
    this.store.dispatch(OrderActions.loadWarehouses({ city }));
  }

  pickWarehouse(w: NovaPoshtaWarehouseDto, details: HTMLDetailsElement): void {
    this.selectedWarehouse.set(w);
    details.open = false;
  }

  canSubmit(): boolean {
    return !!(this.recipientName && this.isPhoneVerified() && this.city && this.selectedWarehouse() && this.orderIds().length > 0);
  }

  submit(): void {
    const w = this.selectedWarehouse();
    if (!w) return;
    this.store.dispatch(
      OrderActions.checkoutAndPayBatch({
        orderIds: this.orderIds(),
        delivery: {
          recipientName: this.recipientName,
          phone: this.phone,
          city: this.city,
          warehouseNumber: w.number,
          warehouseAddress: w.address,
        },
      }),
    );
  }
}
