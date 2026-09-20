import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { Store } from '@ngrx/store';
import { OrderSummaryDto } from '../../core/models';
import { isOrderInProgress, orderStatusLabel, orderStatusTagClass, orderStepLink } from '../../core/order-status';
import {
  OrderActions,
  selectActiveOrders,
  selectArchivedOrders,
  selectOrderBusy,
  selectOrderError,
} from '../../core/state/order';

const CART_ELIGIBLE_STATUSES = ['ReviewReady', 'AwaitingPayment'];

@Component({
  selector: 'app-orders',
  standalone: true,
  imports: [DatePipe, RouterLink],
  template: `
    <div class="page">
      <div style="display: flex; align-items: baseline; justify-content: space-between; gap: var(--space-3);">
        <h2 style="font-size: 28px;">Мої замовлення</h2>
        <button class="btn btn-primary" (click)="createOrder()">Створити календар</button>
      </div>

      @if (error()) {
        <p style="color: var(--color-accent-2-700); font-size: 13px;">{{ error() }}</p>
      }

      @if (activeOrders().length === 0 && archivedOrders().length === 0) {
        <p class="text-muted" style="margin-top: var(--space-4);">
          {{ busy() ? 'Завантажуємо…' : 'Тут поки порожньо. Створіть свій перший календар.' }}
        </p>
      } @else {
        @if (activeOrders().length === 0) {
          <p class="text-muted" style="margin-top: var(--space-4);">Активних замовлень немає.</p>
        } @else {
          <div style="display: flex; flex-direction: column; gap: var(--space-3); margin-top: var(--space-4);">
            @for (o of activeOrders(); track o.id) {
              <div class="card" style="flex-direction: row; align-items: center; gap: var(--space-3);">
                @if (isCartEligible(o)) {
                  <input
                    type="checkbox"
                    style="width: 18px; height: 18px; flex: none; cursor: pointer;"
                    [checked]="isSelected(o.id)"
                    (change)="toggleSelect(o.id)"
                  />
                } @else {
                  <span
                    class="tag"
                    style="flex: none; background: var(--color-accent-2-100); color: var(--color-accent-2-700); font-size: 10.5px; white-space: nowrap;"
                  >
                    Не готово
                  </span>
                }

                <a
                  [routerLink]="stepLink(o)"
                  style="flex: 1; min-width: 0; display: flex; align-items: center; gap: var(--space-3); text-decoration: none; color: inherit;"
                >
                  @if (o.coverImageUrl) {
                    <img
                      [src]="o.coverImageUrl"
                      alt="Обкладинка календаря"
                      style="width: 64px; height: 64px; object-fit: cover; border-radius: var(--radius-sm); flex: none;"
                    />
                  }
                  <div style="flex: 1; min-width: 0;">
                    <div class="card-title">{{ o.styleName || 'Без стилю' }}</div>
                    <div class="card-meta">
                      <span>{{ o.createdAtUtc | date: 'dd.MM.yyyy' }}</span>
                      <span>·</span>
                      <span>{{ o.price }} ₴</span>
                    </div>
                  </div>
                  <span [class]="tagClass(o)">{{ statusLabel(o) }}</span>
                  <span class="text-muted" style="font-size: 13px; white-space: nowrap;">
                    {{ inProgress(o) ? 'Продовжити' : 'Статус' }}
                  </span>
                </a>

                @if (isCartEligible(o)) {
                  <div style="display: flex; align-items: center; gap: 4px; flex: none;">
                    <span class="text-muted" style="font-size: 11px;">К-сть</span>
                    <input
                      type="number"
                      min="1"
                      max="20"
                      [value]="quantityFor(o)"
                      (change)="onQuantityChange(o.id, $event)"
                      style="width: 52px; padding: 4px 6px; border: 1px solid var(--color-divider); border-radius: var(--radius-sm); font-size: 13px;"
                    />
                  </div>
                }

                <button
                  type="button"
                  class="btn btn-ghost"
                  style="flex: none;"
                  (click)="archive(o)"
                >
                  Архівувати
                </button>
              </div>
            }
          </div>
        }

        @if (selectedCount() > 0) {
          <div
            class="card"
            style="flex-direction: row; align-items: center; justify-content: space-between; gap: var(--space-3); margin-top: var(--space-3); position: sticky; bottom: var(--space-3); box-shadow: var(--shadow-lg);"
          >
            <div>
              <div style="font-size: 13px;" class="text-muted">Обрано: {{ selectedCount() }}</div>
              <div class="money" style="font-size: 22px; font-weight: 500;">{{ totalPrice() }} ₴</div>
            </div>
            <button class="btn btn-primary" (click)="proceedToCheckout()">Оформити</button>
          </div>
        }

        @if (archivedOrders().length > 0) {
          <div class="hr"></div>
          <button type="button" class="btn btn-secondary" (click)="showArchived.set(!showArchived())">
            {{ showArchived() ? 'Сховати архівовані' : 'Архівовані' }} ({{ archivedOrders().length }})
          </button>

          @if (showArchived()) {
            <div style="display: flex; flex-direction: column; gap: var(--space-3); margin-top: var(--space-3);">
              @for (o of archivedOrders(); track o.id) {
                <div class="card" style="flex-direction: row; align-items: center; gap: var(--space-3); opacity: 0.7;">
                  @if (o.coverImageUrl) {
                    <img
                      [src]="o.coverImageUrl"
                      alt="Обкладинка календаря"
                      style="width: 64px; height: 64px; object-fit: cover; border-radius: var(--radius-sm); flex: none;"
                    />
                  }
                  <div style="flex: 1; min-width: 0;">
                    <div class="card-title">{{ o.styleName || 'Без стилю' }}</div>
                    <div class="card-meta">
                      <span>{{ o.createdAtUtc | date: 'dd.MM.yyyy' }}</span>
                      <span>·</span>
                      <span>{{ o.price }} ₴</span>
                    </div>
                  </div>
                  <span [class]="tagClass(o)">{{ statusLabel(o) }}</span>
                  <button type="button" class="btn btn-secondary" style="flex: none;" (click)="unarchive(o)">
                    Відновити
                  </button>
                </div>
              }
            </div>
          }
        }
      }
    </div>
  `,
})
export class OrdersComponent implements OnInit {
  private readonly store = inject(Store);
  private readonly router = inject(Router);

  readonly activeOrders = this.store.selectSignal(selectActiveOrders);
  readonly archivedOrders = this.store.selectSignal(selectArchivedOrders);
  readonly busy = this.store.selectSignal(selectOrderBusy);
  readonly error = this.store.selectSignal(selectOrderError);
  readonly showArchived = signal(false);

  readonly selected = signal<Set<string>>(new Set());
  private readonly quantityOverrides = signal<Map<string, number>>(new Map());

  readonly selectedCount = computed(() => this.selected().size);
  readonly totalPrice = computed(() => {
    const selected = this.selected();
    const overrides = this.quantityOverrides();
    return this.activeOrders()
      .filter((o) => selected.has(o.id))
      .reduce((sum, o) => sum + o.price * (overrides.get(o.id) ?? o.printQuantity), 0);
  });

  ngOnInit(): void {
    this.store.dispatch(OrderActions.loadMyOrders());
  }

  statusLabel = (o: OrderSummaryDto) => orderStatusLabel(o.status);
  tagClass = (o: OrderSummaryDto) => orderStatusTagClass(o.status);
  stepLink = (o: OrderSummaryDto) => orderStepLink(o.id, o.status);
  inProgress = (o: OrderSummaryDto) => isOrderInProgress(o.status);
  isCartEligible = (o: OrderSummaryDto) => CART_ELIGIBLE_STATUSES.includes(o.status);
  isSelected = (orderId: string) => this.selected().has(orderId);
  quantityFor = (o: OrderSummaryDto) => this.quantityOverrides().get(o.id) ?? o.printQuantity;

  toggleSelect(orderId: string): void {
    const next = new Set(this.selected());
    if (next.has(orderId)) {
      next.delete(orderId);
    } else {
      next.add(orderId);
    }
    this.selected.set(next);
  }

  onQuantityChange(orderId: string, event: Event): void {
    const raw = Number((event.target as HTMLInputElement).value);
    const quantity = Math.min(20, Math.max(1, Math.round(raw) || 1));
    const next = new Map(this.quantityOverrides());
    next.set(orderId, quantity);
    this.quantityOverrides.set(next);
    this.store.dispatch(OrderActions.setPrintQuantity({ orderId, quantity }));
  }

  createOrder(): void {
    // The order itself isn't created until a photo is actually uploaded — see #348.
    this.router.navigate(['/order', 'new', 'upload']);
  }

  archive(o: OrderSummaryDto): void {
    this.store.dispatch(OrderActions.archiveOrder({ orderId: o.id }));
  }

  unarchive(o: OrderSummaryDto): void {
    this.store.dispatch(OrderActions.unarchiveOrder({ orderId: o.id }));
  }

  proceedToCheckout(): void {
    const ids = Array.from(this.selected());
    if (ids.length === 0) return;
    this.router.navigate(['/checkout-batch'], { queryParams: { ids: ids.join(',') } });
  }
}
