import { Component, OnDestroy, OnInit, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { Subscription, interval, startWith, switchMap } from 'rxjs';
import { OrderService } from '../../core/order.service';
import { OrderDto, SheetDto } from '../../core/models';

const MONTH_NAMES = [
  'Січень', 'Лютий', 'Березень', 'Квітень', 'Травень', 'Червень',
  'Липень', 'Серпень', 'Вересень', 'Жовтень', 'Листопад', 'Грудень',
];

@Component({
  selector: 'app-month',
  standalone: true,
  template: `
    <div class="page page-narrow">
      @if (order(); as o) {
        <div class="step-label"><span>Аркуш {{ monthNumber }} із 12</span></div>
        <h2 style="font-size: 28px;">{{ monthName }}</h2>

        @if (sheet(); as s) {
          @if (s.status === 'Failed') {
            <div class="card" style="border: 1px solid var(--color-accent-2-300); background: var(--color-accent-2-100);">
              <div class="card-title">Не вдалося згенерувати цей аркуш</div>
              <p class="card-body">Спробуйте ще раз — це не витрачає перегенерацію.</p>
              <button class="btn btn-primary" style="align-self: flex-start;" [disabled]="busy()" (click)="regenerate(s)">Спробувати ще раз</button>
            </div>
          } @else if (s.imageUrl) {
            <div style="aspect-ratio: 3/4; background-size: cover; background-position: center; border-radius: var(--radius-md);"
                 [style.background-image]="'url(' + s.imageUrl + ')'"></div>
          } @else {
            <div class="sheet-thumb generating" style="aspect-ratio: 3/4; width: 100%;"></div>
            <p class="text-muted" style="font-size: 13px; margin-top: var(--space-2);">Цей аркуш ще генерується…</p>
          }

          @if (personalDatesForMonth(o).length > 0) {
            <p class="text-muted" style="font-size: 12.5px; margin-top: var(--space-2);">
              У сітці цього місяця: {{ personalDatesForMonth(o).join(', ') }}
            </p>
          }

          <p class="text-muted" style="font-size: 11.5px;">Залишилось {{ o.regenerationsRemaining }} перегенерацій</p>

          @if (error()) {
            <p style="color: var(--color-accent-2-700); font-size: 13px;">{{ error() }}</p>
          }

          <div style="display: flex; gap: 10px; margin-top: var(--space-3);">
            @if (s.status === 'Ready') {
              <button class="btn btn-secondary" style="flex: 1; min-height: 48px;" [disabled]="busy()" (click)="regenerate(s)">
                Перегенерувати
              </button>
            }
            <button class="btn btn-primary" style="flex: 1; min-height: 48px;" [disabled]="busy()" (click)="next()">
              {{ monthNumber < 12 ? 'Далі' : 'До огляду' }}
            </button>
          </div>
        }
      }
    </div>
  `,
})
export class MonthComponent implements OnInit, OnDestroy {
  readonly order = signal<OrderDto | null>(null);
  readonly busy = signal(false);
  readonly error = signal<string | null>(null);
  readonly orderId: string;
  monthNumber = 1;
  private sub?: Subscription;
  private paramSub?: Subscription;

  constructor(
    private readonly route: ActivatedRoute,
    private readonly router: Router,
    private readonly orders: OrderService,
  ) {
    this.orderId = this.route.snapshot.paramMap.get('orderId')!;
    this.monthNumber = Number(this.route.snapshot.paramMap.get('month'));
  }

  get monthName(): string {
    return MONTH_NAMES[this.monthNumber - 1] ?? '';
  }

  ngOnInit(): void {
    // The month route is reused between /months/1 and /months/2 etc, so the component
    // instance survives navigation — re-read the param instead of relying on the constructor.
    this.paramSub = this.route.paramMap.subscribe((pm) => {
      this.monthNumber = Number(pm.get('month'));
    });

    this.sub = interval(1500)
      .pipe(
        startWith(0),
        switchMap(() => this.orders.getOrder(this.orderId)),
      )
      .subscribe((o) => this.order.set(o));
  }

  ngOnDestroy(): void {
    this.sub?.unsubscribe();
    this.paramSub?.unsubscribe();
  }

  sheet(): SheetDto | undefined {
    return this.order()?.sheets.find((s) => s.kind === 'Month' && s.index === this.monthNumber);
  }

  personalDatesForMonth(o: OrderDto): string[] {
    return o.personalDates
      .filter((d) => d.month === this.monthNumber)
      .map((d) => `${d.day.toString().padStart(2, '0')}.${d.month.toString().padStart(2, '0')} — ${d.label}`);
  }

  regenerate(s: SheetDto): void {
    this.busy.set(true);
    this.orders.regenerateSheet(this.orderId, s.id).subscribe({
      next: (o) => {
        this.order.set(o);
        this.busy.set(false);
      },
      error: () => {
        this.error.set('Перегенерації вичерпано.');
        this.busy.set(false);
      },
    });
  }

  next(): void {
    if (this.monthNumber < 12) {
      this.router.navigate(['/order', this.orderId, 'months', this.monthNumber + 1]);
    } else {
      this.router.navigate(['/order', this.orderId, 'review']);
    }
  }
}
