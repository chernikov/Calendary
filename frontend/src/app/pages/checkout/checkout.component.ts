import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { Store } from '@ngrx/store';
import {
  OrderActions,
  selectCities,
  selectOrder,
  selectOrderBusy,
  selectOrderError,
  selectWarehouses,
} from '../../core/state/order';
import { AuthService } from '../../core/auth.service';
import { NovaPoshtaWarehouseDto, OrderDto } from '../../core/models';

@Component({
  selector: 'app-checkout',
  standalone: true,
  imports: [FormsModule, RouterLink],
  template: `
    <div class="page page-narrow">
      @if (order(); as o) {
        <div style="display: flex; gap: var(--space-3); margin-bottom: var(--space-2); font-size: 13.5px;">
          <a [routerLink]="['/order', o.id, 'review']" style="display: inline-flex; align-items: center; gap: 4px;">
            ← До перегляду
          </a>
          <a [routerLink]="['/order', o.id, 'style']" style="display: inline-flex; align-items: center; gap: 4px;">
            Змінити образи
          </a>
        </div>

        <h2 style="font-size: 28px;">Куди доставити</h2>
        <p class="text-muted">Доставка Новою поштою входить у ціну.</p>

        <div style="display: flex; flex-direction: column; gap: 14px; margin: var(--space-3) 0;">
          <div class="field">
            <label>Отримувач</label>
            <input class="input" [(ngModel)]="recipientName" />
          </div>
          <div class="field">
            <label>Телефон</label>
            <input class="input" [(ngModel)]="phone" placeholder="+380 67 000 00 00" />
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

        <div class="field">
          <label>Промокод</label>
          @if (o.promoCode) {
            <div style="display: flex; align-items: center; gap: 8px;">
              <span class="tag tag-accent">{{ o.promoCode }}</span>
              <span class="text-muted" style="font-size: 12.5px;">−{{ o.discountAmount }} ₴</span>
              <button class="btn btn-ghost" style="padding: 0; font-size: 12.5px;" [disabled]="busy()" (click)="removePromo()">
                Прибрати
              </button>
            </div>
          } @else {
            <div style="display: flex; gap: 8px;">
              <input class="input" [(ngModel)]="promoCodeInput" placeholder="Введіть код" style="flex: 1;" />
              <button class="btn btn-secondary" [disabled]="!promoCodeInput.trim() || busy()" (click)="applyPromo()">
                Застосувати
              </button>
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
          Оплатити {{ payable(o) }} ₴ через monobank
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
  promoCodeInput = '';

  private readonly orderId: string;
  private cityDebounce?: ReturnType<typeof setTimeout>;

  constructor(
    private readonly route: ActivatedRoute,
    readonly auth: AuthService,
  ) {
    this.orderId = this.route.snapshot.paramMap.get('orderId')!;
  }

  ngOnInit(): void {
    this.store.dispatch(OrderActions.loadOrder({ orderId: this.orderId }));
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
    return !!(this.recipientName && this.phone && this.city && this.selectedWarehouse());
  }

  payable(o: OrderDto): number {
    return Math.max(0, o.price * o.printQuantity - o.discountAmount);
  }

  applyPromo(): void {
    const code = this.promoCodeInput.trim();
    if (!code) return;
    this.store.dispatch(OrderActions.applyPromoCode({ orderId: this.orderId, code }));
  }

  removePromo(): void {
    this.promoCodeInput = '';
    this.store.dispatch(OrderActions.removePromoCode({ orderId: this.orderId }));
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
      }),
    );
  }
}
