import { Component, OnInit, inject } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { Store } from '@ngrx/store';
import { OrderActions, selectOrder } from '../../core/state/order';
import { OrderDto } from '../../core/models';

const MONTH_NAMES = [
  'Січень', 'Лютий', 'Березень', 'Квітень', 'Травень', 'Червень',
  'Липень', 'Серпень', 'Вересень', 'Жовтень', 'Листопад', 'Грудень',
];

@Component({
  selector: 'app-review',
  standalone: true,
  template: `
    <div class="page">
      <h2 style="font-size: 28px;">Ваш календар</h2>

      @if (order(); as o) {
        @if (o.status !== 'ReviewReady' && !isPastReview(o)) {
          <p class="text-muted">Ще не всі аркуші готові — поверніться, коли генерація завершиться.</p>
        }

        <div style="display: grid; grid-template-columns: repeat(auto-fill, minmax(110px, 1fr)); gap: var(--space-2); margin: var(--space-4) 0;">
          @for (sheet of o.sheets; track sheet.id) {
            <a
              [routerLink]="sheet.kind === 'Cover' ? ['/order', o.id, 'cover'] : ['/order', o.id, 'months', sheet.index]"
              style="text-decoration: none; color: inherit;"
            >
              @if (sheet.imageUrl) {
                <div style="aspect-ratio: 3/4; background-size: cover; background-position: center; border-radius: var(--radius-sm);"
                     [style.background-image]="'url(' + sheet.imageUrl + ')'"></div>
              } @else {
                <div class="sheet-thumb" style="width: 100%; height: auto; aspect-ratio: 3/4;"></div>
              }
              <div style="font-size: 11px; margin-top: 4px; text-align: center;">
                {{ sheet.kind === 'Cover' ? 'Обкладинка' : monthName(sheet.index) }}
              </div>
            </a>
          }
        </div>

        <div class="hr"></div>

        <div style="display: flex; justify-content: space-between; align-items: baseline; padding: 16px 0;">
          <span class="d" style="font-family: var(--font-heading); font-weight: 600; font-size: 17px;">До сплати</span>
          <span class="money" style="font-size: 30px; font-weight: 500;">{{ o.price }} ₴</span>
        </div>

        <button class="btn btn-primary btn-block" style="max-width: 320px;" (click)="proceed(o)">До оплати</button>
      }
    </div>
  `,
  imports: [RouterLink],
})
export class ReviewComponent implements OnInit {
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
    this.store.dispatch(OrderActions.loadOrder({ orderId: this.orderId }));
  }

  monthName(index: number): string {
    return MONTH_NAMES[index - 1] ?? '';
  }

  isPastReview(o: OrderDto): boolean {
    return ['AwaitingPayment', 'Paid', 'Printing', 'Shipped', 'Delivered'].includes(o.status);
  }

  proceed(o: OrderDto): void {
    this.router.navigate(['/order', o.id, 'checkout']);
  }
}
