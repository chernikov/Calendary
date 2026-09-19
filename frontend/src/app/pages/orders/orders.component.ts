import { Component, OnInit, inject, signal } from '@angular/core';
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
              <a
                class="card selectable"
                style="flex-direction: row; align-items: center; gap: var(--space-3); text-decoration: none; color: inherit;"
                [routerLink]="stepLink(o)"
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
                <button
                  type="button"
                  class="btn btn-ghost"
                  style="flex: none;"
                  (click)="$event.preventDefault(); $event.stopPropagation(); archive(o)"
                >
                  Архівувати
                </button>
              </a>
            }
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

  ngOnInit(): void {
    this.store.dispatch(OrderActions.loadMyOrders());
  }

  statusLabel = (o: OrderSummaryDto) => orderStatusLabel(o.status);
  tagClass = (o: OrderSummaryDto) => orderStatusTagClass(o.status);
  stepLink = (o: OrderSummaryDto) => orderStepLink(o.id, o.status);
  inProgress = (o: OrderSummaryDto) => isOrderInProgress(o.status);

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
}
