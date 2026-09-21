import { Component, OnInit, inject } from '@angular/core';
import { DatePipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { Store } from '@ngrx/store';
import { OrderSummaryDto } from '../../core/models';
import { orderStatusLabel, orderStatusTagClass } from '../../core/order-status';
import { OrderActions, selectOrderBusy, selectOrderError, selectOrderHistory } from '../../core/state/order';

@Component({
    selector: 'app-order-history',
    imports: [DatePipe, RouterLink],
    template: `
    <div class="page">
      <h2 style="font-size: 28px;">Мої замовлення</h2>
      <p class="text-muted">Оплачені календарі — тут можна відстежити друк і доставку.</p>

      @if (error()) {
        <p style="color: var(--color-accent-2-700); font-size: 13px;">{{ error() }}</p>
      }

      @if (orders().length === 0) {
        <p class="text-muted" style="margin-top: var(--space-4);">
          {{ busy() ? 'Завантажуємо…' : 'Тут поки порожньо — оплачені замовлення з'являться тут.' }}
        </p>
      } @else {
        <div style="display: flex; flex-direction: column; gap: var(--space-3); margin-top: var(--space-4);">
          @for (o of orders(); track o.id) {
            <a
              class="card selectable"
              [routerLink]="['/order', o.id, 'status']"
              style="flex-direction: row; flex-wrap: wrap; align-items: center; gap: var(--space-3); text-decoration: none; color: inherit;"
            >
              @if (o.coverImageUrl) {
                <img
                  [src]="o.coverImageUrl"
                  alt="Обкладинка календаря"
                  style="width: 64px; height: 64px; object-fit: cover; border-radius: var(--radius-sm); flex: none;"
                />
              }
              <div style="flex: 1 1 200px; min-width: 0;">
                <div class="card-title">{{ o.styleName || 'Без стилю' }}</div>
                <div class="card-meta">
                  <span>{{ o.createdAtUtc | date: 'dd.MM.yyyy' }}</span>
                  <span>·</span>
                  <span>{{ o.price * o.printQuantity }} ₴</span>
                </div>
              </div>
              <span [class]="tagClass(o)">{{ statusLabel(o) }}</span>
            </a>
          }
        </div>
      }
    </div>
  `
})
export class OrderHistoryComponent implements OnInit {
  private readonly store = inject(Store);

  readonly orders = this.store.selectSignal(selectOrderHistory);
  readonly busy = this.store.selectSignal(selectOrderBusy);
  readonly error = this.store.selectSignal(selectOrderError);

  ngOnInit(): void {
    this.store.dispatch(OrderActions.loadMyOrders());
  }

  statusLabel = (o: OrderSummaryDto) => orderStatusLabel(o.status);
  tagClass = (o: OrderSummaryDto) => orderStatusTagClass(o.status);
}
