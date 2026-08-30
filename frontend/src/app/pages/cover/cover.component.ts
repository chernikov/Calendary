import { Component, OnDestroy, OnInit, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ActivatedRoute, Router } from '@angular/router';
import { Store } from '@ngrx/store';
import { Actions, ofType } from '@ngrx/effects';
import { OrderActions, selectCoverSheet, selectOrder, selectOrderBusy, selectOrderError } from '../../core/state/order';
import { SheetDto } from '../../core/models';

@Component({
  selector: 'app-cover',
  standalone: true,
  template: `
    <div class="page page-narrow">
      <div class="step-label"><span>Крок 5 із 5</span></div>
      <h2 style="font-size: 28px;">Обкладинка</h2>
      <p class="text-muted">Вона задає стиль усіх дванадцяти місяців. Після підтвердження змінити її не вийде.</p>

      @if (cover(); as c) {
        @if (c.imageUrl) {
          <div style="aspect-ratio: 3/4; background-size: cover; background-position: center; border-radius: var(--radius-md); border: 1.5px solid var(--color-accent);"
               [style.background-image]="'url(' + c.imageUrl + ')'"></div>
        } @else {
          <div class="sheet-thumb generating" style="aspect-ratio: 3/4; width: 100%;"></div>
          <p class="text-muted" style="font-size: 13px; margin-top: var(--space-2);">Обкладинка ще генерується…</p>
        }

        <p class="text-muted" style="font-size: 11.5px; margin-top: var(--space-2);">
          Залишилось {{ order()?.regenerationsRemaining }} перегенерацій
        </p>

        @if (error()) {
          <p style="color: var(--color-accent-2-700); font-size: 13px;">{{ error() }}</p>
        }

        <div style="display: flex; gap: 10px; margin-top: var(--space-3);">
          <button class="btn btn-secondary" style="flex: 1; min-height: 48px;" [disabled]="!c.imageUrl || busy()" (click)="regenerate(c)">
            Перегенерувати
          </button>
          <button class="btn btn-primary" style="flex: 1; min-height: 48px;" [disabled]="!c.imageUrl || busy()" (click)="confirm(c)">
            Обрати цю
          </button>
        </div>
      }
    </div>
  `,
})
export class CoverComponent implements OnInit, OnDestroy {
  private readonly store = inject(Store);
  readonly order = this.store.selectSignal(selectOrder);
  readonly cover = this.store.selectSignal(selectCoverSheet);
  readonly busy = this.store.selectSignal(selectOrderBusy);
  readonly error = this.store.selectSignal(selectOrderError);
  private readonly orderId: string;

  constructor(
    private readonly route: ActivatedRoute,
    private readonly router: Router,
    private readonly actions$: Actions,
  ) {
    this.orderId = this.route.snapshot.paramMap.get('orderId')!;
    this.actions$
      .pipe(ofType(OrderActions.confirmCoverSuccess), takeUntilDestroyed())
      .subscribe(() => this.router.navigate(['/order', this.orderId, 'months', 1]));
  }

  ngOnInit(): void {
    this.store.dispatch(OrderActions.startOrderPolling({ orderId: this.orderId, intervalMs: 1500 }));
  }

  ngOnDestroy(): void {
    this.store.dispatch(OrderActions.stopOrderPolling());
  }

  regenerate(c: SheetDto): void {
    this.store.dispatch(OrderActions.regenerateSheet({ orderId: this.orderId, sheetId: c.id }));
  }

  confirm(c: SheetDto): void {
    this.store.dispatch(OrderActions.confirmCover({ orderId: this.orderId, sheetId: c.id }));
  }
}
