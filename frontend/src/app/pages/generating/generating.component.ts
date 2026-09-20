import { Component, OnDestroy, OnInit, inject } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { Store } from '@ngrx/store';
import { OrderActions, selectDownloadingPdf, selectOrder } from '../../core/state/order';
import { OrderDto, PersonalDateDto } from '../../core/models';

const REQUIRED_SHEET_COUNT = 13;

@Component({
  selector: 'app-generating',
  standalone: true,
  imports: [RouterLink],
  template: `
    <div class="page">
      <div class="step-label"><span>Крок 4 із 5</span></div>
      @if (order(); as o) {
        <a
          [routerLink]="['/order', o.id, 'style']"
          style="display: inline-flex; align-items: center; gap: 4px; margin-bottom: var(--space-2); font-size: 13.5px;"
        >
          ← Повернутись до конструктора
        </a>

        <h2 style="font-size: 34px;">Готуємо {{ REQUIRED_SHEET_COUNT }} аркушів</h2>
        <p class="text-muted" style="max-width: 520px;">
          Це займе кілька хвилин. Можете закрити сторінку — посилання на замовлення приведе рівно сюди.
        </p>
        <p class="text-muted" style="font-size: 13px;">Готово {{ readyCount(o) }} із {{ REQUIRED_SHEET_COUNT }}</p>

        @if (o.sheets.length < REQUIRED_SHEET_COUNT) {
          <p style="color: var(--color-accent-2-700); font-size: 13px;">
            Обрано образи ще не для всіх аркушів ({{ o.sheets.length }} із {{ REQUIRED_SHEET_COUNT }}) — поверніться до
            образів і завершіть вибір для решти місяців.
          </p>
        }

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

        @if (personalDates(o).length > 0) {
          <div class="hr"></div>
          <h3 style="font-size: 17px;">Персональні дати</h3>
          <div style="display: flex; flex-direction: column; gap: 4px;">
            @for (d of personalDates(o); track d.id) {
              <div style="font-size: 13.5px;">
                <span class="money" style="color: var(--color-accent-700);">{{ pad(d.day) }}.{{ pad(d.month) }}</span>
                — {{ d.label }}
              </div>
            }
          </div>
        }

        @if (o.sheets.length === REQUIRED_SHEET_COUNT && readyCount(o) === REQUIRED_SHEET_COUNT) {
          <div style="display: flex; gap: var(--space-2); margin-top: var(--space-4);">
            <button
              class="btn btn-secondary"
              style="min-height: 48px; font-size: 15px; padding-inline: 22px;"
              [disabled]="downloadingPdf()"
              (click)="generatePdf(o)"
            >
              {{ downloadingPdf() ? 'Генеруємо…' : 'Згенерувати PDF' }}
            </button>
            <button
              class="btn btn-primary"
              style="min-height: 48px; font-size: 15px; padding-inline: 26px;"
              (click)="goToPayment()"
            >
              Перейти до оплати
            </button>
          </div>
        }
      }
    </div>
  `,
})
export class GeneratingComponent implements OnInit, OnDestroy {
  private readonly store = inject(Store);
  readonly order = this.store.selectSignal(selectOrder);
  readonly downloadingPdf = this.store.selectSignal(selectDownloadingPdf);
  readonly REQUIRED_SHEET_COUNT = REQUIRED_SHEET_COUNT;
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

  personalDates(o: OrderDto): PersonalDateDto[] {
    return [...o.personalDates].sort((a, b) => a.month - b.month || a.day - b.day);
  }

  generatePdf(o: OrderDto): void {
    this.store.dispatch(OrderActions.downloadPdf({ orderId: o.id }));
  }

  // Straight to the cart (see #377/#395) — no PDF download needed first, the order is already
  // ReviewReady/selectable there alongside whatever else the customer might have in progress.
  goToPayment(): void {
    this.router.navigate(['/orders']);
  }
}
