import { Component, OnDestroy, OnInit, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ActivatedRoute, Router } from '@angular/router';
import { Store } from '@ngrx/store';
import { Actions, ofType } from '@ngrx/effects';
import { OrderActions, selectDownloadingPdf, selectOrder } from '../../core/state/order';
import { OrderDto, PersonalDateDto } from '../../core/models';

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

        @if (readyCount(o) === o.sheets.length) {
          <button
            class="btn btn-primary"
            style="min-height: 48px; font-size: 15px; padding-inline: 26px; margin-top: var(--space-4);"
            [disabled]="downloadingPdf()"
            (click)="generateCalendar(o)"
          >
            {{ downloadingPdf() ? 'Генеруємо…' : 'Генерувати календар' }}
          </button>
        }
      }
    </div>
  `,
})
export class GeneratingComponent implements OnInit, OnDestroy {
  private readonly store = inject(Store);
  readonly order = this.store.selectSignal(selectOrder);
  readonly downloadingPdf = this.store.selectSignal(selectDownloadingPdf);
  private readonly orderId: string;

  constructor(
    private readonly route: ActivatedRoute,
    private readonly router: Router,
    private readonly actions$: Actions,
  ) {
    this.orderId = this.route.snapshot.paramMap.get('orderId')!;
    // "Генерувати календар" downloads the (watermarked, pre-payment) PDF and moves straight to
    // checkout — cover-confirm/months/review are no longer part of the primary flow.
    this.actions$
      .pipe(ofType(OrderActions.downloadPdfSuccess), takeUntilDestroyed())
      .subscribe(() => this.router.navigate(['/order', this.orderId, 'checkout']));
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

  generateCalendar(o: OrderDto): void {
    this.store.dispatch(OrderActions.downloadPdf({ orderId: o.id }));
  }
}
