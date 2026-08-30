import { Component, OnDestroy, OnInit, inject } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { Store } from '@ngrx/store';
import { OrderActions, selectOrder } from '../../core/state/order';
import { OrderDto } from '../../core/models';

@Component({
  selector: 'app-generating',
  standalone: true,
  template: `
    <div class="page">
      <div class="step-label"><span>Крок 4 із 5</span></div>
      @if (order(); as o) {
        <h2 style="font-size: 34px;">Готуємо {{ o.sheets.length }} аркушів</h2>
        <p class="text-muted" style="max-width: 520px;">
          Це займе кілька хвилин. Можете закрити сторінку — посилання на замовлення приведе рівно сюди.
        </p>
        <p class="text-muted" style="font-size: 13px;">Готово {{ readyCount(o) }} із {{ o.sheets.length }}</p>

        <div style="display: grid; grid-template-columns: repeat(auto-fill, minmax(70px, 1fr)); gap: 12px; max-width: 720px; margin: var(--space-4) 0;">
          @for (sheet of o.sheets; track sheet.id) {
            <div>
              @if (sheet.imageUrl) {
                <div class="sheet-thumb ready" style="width: 100%; height: 90px;" [style.background-image]="'url(' + sheet.imageUrl + ')'"></div>
              } @else if (sheet.status === 'Generating') {
                <div class="sheet-thumb generating" style="width: 100%; height: 90px;"></div>
              } @else {
                <div class="sheet-thumb" style="width: 100%; height: 90px;"></div>
              }
              <div class="money" style="font-size: 10px; color: var(--color-neutral-600); margin-top: 4px; text-align: center;">
                {{ sheet.kind === 'Cover' ? 'ОБК' : pad(sheet.index) }}
              </div>
            </div>
          }
        </div>

        @if (o.status === 'CoverReady' || o.status === 'CoverConfirmed' || o.status === 'ReviewReady') {
          <button class="btn btn-primary" style="min-height: 48px; font-size: 15px; padding-inline: 26px;" (click)="proceed(o)">
            Обрати обкладинку
          </button>
        }
      }
    </div>
  `,
})
export class GeneratingComponent implements OnInit, OnDestroy {
  private readonly store = inject(Store);
  readonly order = this.store.selectSignal(selectOrder);
  private readonly orderId: string;

  constructor(
    private readonly route: ActivatedRoute,
    private readonly router: Router,
  ) {
    this.orderId = this.route.snapshot.paramMap.get('orderId')!;
  }

  ngOnInit(): void {
    this.store.dispatch(OrderActions.startOrderPolling({ orderId: this.orderId, intervalMs: 1500 }));
  }

  ngOnDestroy(): void {
    this.store.dispatch(OrderActions.stopOrderPolling());
  }

  readyCount(o: OrderDto): number {
    return o.sheets.filter((s) => s.status === 'Ready').length;
  }

  pad(n: number): string {
    return n.toString().padStart(2, '0');
  }

  proceed(o: OrderDto): void {
    this.router.navigate(['/order', o.id, 'cover']);
  }
}
