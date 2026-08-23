import { Component, OnDestroy, OnInit, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { Subscription, interval, startWith, switchMap } from 'rxjs';
import { OrderService } from '../../core/order.service';
import { OrderDto, SheetDto } from '../../core/models';

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
  readonly order = signal<OrderDto | null>(null);
  readonly busy = signal(false);
  readonly error = signal<string | null>(null);
  private readonly orderId: string;
  private sub?: Subscription;

  constructor(
    private readonly route: ActivatedRoute,
    private readonly router: Router,
    private readonly orders: OrderService,
  ) {
    this.orderId = this.route.snapshot.paramMap.get('orderId')!;
  }

  ngOnInit(): void {
    this.sub = interval(1500)
      .pipe(
        startWith(0),
        switchMap(() => this.orders.getOrder(this.orderId)),
      )
      .subscribe((o) => this.order.set(o));
  }

  ngOnDestroy(): void {
    this.sub?.unsubscribe();
  }

  cover(): SheetDto | undefined {
    return this.order()?.sheets.find((s) => s.kind === 'Cover');
  }

  regenerate(c: SheetDto): void {
    this.busy.set(true);
    this.orders.regenerateSheet(this.orderId, c.id).subscribe({
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

  confirm(c: SheetDto): void {
    this.busy.set(true);
    this.orders.confirmCover(this.orderId, c.id).subscribe({
      next: () => this.router.navigate(['/order', this.orderId, 'months', 1]),
      error: () => {
        this.error.set('Не вдалося підтвердити обкладинку.');
        this.busy.set(false);
      },
    });
  }
}
