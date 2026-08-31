import { Component, OnDestroy, OnInit, inject } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { Store } from '@ngrx/store';
import { OrderActions, selectDownloadingPdf, selectOrder, selectOrderBusy } from '../../core/state/order';
import { OrderDto, OrderStatus } from '../../core/models';

const TIMELINE: { status: OrderStatus; label: string }[] = [
  { status: 'Paid', label: 'Оплачено' },
  { status: 'Printing', label: 'Друкуємо' },
  { status: 'Shipped', label: 'Відправлено' },
  { status: 'Delivered', label: 'Доставлено' },
];

@Component({
  selector: 'app-status',
  standalone: true,
  template: `
    <div class="page page-narrow">
      @if (order(); as o) {
        <h2 style="font-size: 28px;">Статус замовлення</h2>

        @if (o.status === 'Cancelled') {
          <p class="text-muted">Замовлення скасовано.</p>
        } @else {
          <div style="display: flex; flex-direction: column; gap: 0; margin: var(--space-4) 0;">
            @for (step of timeline; track step.status; let last = $last) {
              <div style="display: flex; gap: 12px; align-items: flex-start;">
                <div style="display: flex; flex-direction: column; align-items: center;">
                  <div
                    style="width: 12px; height: 12px; border-radius: 50%; flex: none;"
                    [style.background]="stepIndex(o) >= $index ? 'var(--color-accent)' : 'var(--color-neutral-300)'"
                  ></div>
                  @if (!last) {
                    <div style="width: 1px; flex: 1; min-height: 32px;"
                         [style.background]="stepIndex(o) > $index ? 'var(--color-accent)' : 'var(--color-divider)'"></div>
                  }
                </div>
                <div style="padding-bottom: 24px;">
                  <div [style.color]="stepIndex(o) >= $index ? 'var(--color-text)' : 'var(--color-neutral-500)'">{{ step.label }}</div>
                  @if (step.status === 'Shipped' && o.delivery?.trackingNumber && stepIndex(o) >= $index) {
                    <div class="money text-muted" style="font-size: 12px;">ТТН {{ o.delivery?.trackingNumber }}</div>
                  }
                </div>
              </div>
            }
          </div>
        }

        @if (o.status !== 'Cancelled') {
          <button class="btn btn-secondary" [disabled]="downloadingPdf()" (click)="downloadPdf(o)">Завантажити PDF</button>
        }

        @if (isCancellable(o)) {
          <button class="btn btn-danger" [disabled]="busy()" (click)="cancel(o)">Скасувати замовлення</button>
        }
      }
    </div>
  `,
})
export class StatusComponent implements OnInit, OnDestroy {
  private readonly store = inject(Store);
  readonly order = this.store.selectSignal(selectOrder);
  readonly busy = this.store.selectSignal(selectOrderBusy);
  readonly downloadingPdf = this.store.selectSignal(selectDownloadingPdf);
  readonly timeline = TIMELINE;
  private readonly orderId: string;

  constructor(
    private readonly route: ActivatedRoute,
  ) {
    this.orderId = this.route.snapshot.paramMap.get('orderId')!;
  }

  ngOnInit(): void {
    this.store.dispatch(OrderActions.startOrderPolling({ orderId: this.orderId, intervalMs: 2000 }));
  }

  ngOnDestroy(): void {
    this.store.dispatch(OrderActions.stopOrderPolling());
  }

  stepIndex(o: OrderDto): number {
    return TIMELINE.findIndex((t) => t.status === o.status);
  }

  isCancellable(o: OrderDto): boolean {
    return !['Paid', 'Printing', 'Shipped', 'Delivered', 'Cancelled'].includes(o.status);
  }

  cancel(o: OrderDto): void {
    this.store.dispatch(OrderActions.cancelOrder({ orderId: o.id }));
  }

  downloadPdf(o: OrderDto): void {
    this.store.dispatch(OrderActions.downloadPdf({ orderId: o.id }));
  }
}
