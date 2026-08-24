import { Component, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { OrderService } from '../../core/order.service';
import { OrderDto, StyleCategoryDto } from '../../core/models';

@Component({
  selector: 'app-style-dates',
  standalone: true,
  imports: [FormsModule],
  template: `
    <div class="page">
      <div class="step-label"><span>Крок 3 із 5</span></div>
      <h2 style="font-size: 28px;">Образи</h2>
      <p class="text-muted">Оберіть напрямок для дванадцяти місяців.</p>

      <div style="display: grid; grid-template-columns: repeat(auto-fill, minmax(180px, 1fr)); gap: var(--space-2);">
        @for (cat of categories(); track cat.id) {
          <div class="card selectable" [class.selected]="order()?.styleCategory?.id === cat.id" (click)="pickStyle(cat)">
            <div class="card-title">{{ cat.name }}</div>
            <div class="card-body">{{ cat.description }}</div>
          </div>
        }
      </div>

      <div class="hr"></div>

      <h2 style="font-size: 28px;">Персональні дати</h2>
      <p class="text-muted">
        Дні народження, річниці, важливі дати. Ми надрукуємо їх синім у сітці місяця. Оберіть місяць, щоб додати дату.
      </p>

      <div class="month-grid">
        @for (m of months; track m.number) {
          <button
            type="button"
            class="month-tile"
            [class.has-dates]="datesForMonth(m.number).length > 0"
            (click)="openMonth(m.number)"
          >
            <div class="month-tile-name">{{ m.name }}</div>
            @if (datesForMonth(m.number).length) {
              <div class="month-tile-dates">
                @for (date of datesForMonth(m.number); track date.id) {
                  <span class="month-tile-date">{{ pad(date.day) }} · {{ date.label }}</span>
                }
              </div>
            } @else {
              <div class="month-tile-empty">+ Додати</div>
            }
          </button>
        }
      </div>

      @if (selectedMonth(); as month) {
        <div class="dialog-backdrop" (click)="closeModal()">
          <div class="dialog" (click)="$event.stopPropagation()">
            <div class="dialog-title">{{ monthName(month) }}</div>

            @if (datesForMonth(month).length) {
              <div>
                @for (date of datesForMonth(month); track date.id) {
                  <div style="display: flex; gap: 10px; align-items: center; padding: 8px 0; border-bottom: 1px solid var(--color-divider);">
                    <span class="money" style="font-size: 13px; color: var(--color-accent-700); width: 30px; flex: none;">
                      {{ pad(date.day) }}
                    </span>
                    <span style="font-size: 13.5px; flex: 1;">{{ date.label }}</span>
                    <button class="btn btn-ghost" (click)="removeDate(date.id)">Видалити</button>
                  </div>
                }
              </div>
            }

            <div style="display: flex; gap: 10px; align-items: flex-end; flex-wrap: wrap;">
              <div class="field" style="width: 70px;">
                <label>День</label>
                <input class="input" type="number" min="1" max="31" [(ngModel)]="newDay" />
              </div>
              <div class="field" style="flex: 1; min-width: 180px;">
                <label>Підпис (до 22 символів)</label>
                <input class="input" maxlength="22" [(ngModel)]="newLabel" />
              </div>
            </div>

            @if (error()) {
              <p style="color: var(--color-accent-2-700); font-size: 13px;">{{ error() }}</p>
            }

            <div class="dialog-actions">
              <button class="btn btn-secondary" (click)="closeModal()">Готово</button>
              <button class="btn btn-primary" [disabled]="!newLabel" (click)="addDate()">Додати дату</button>
            </div>
          </div>
        </div>
      }

      <button
        class="btn btn-primary btn-block"
        style="max-width: 320px;"
        [disabled]="!order()?.styleCategory || loading()"
        (click)="startGeneration()"
      >
        Почати генерацію
      </button>
    </div>
  `,
})
export class StyleDatesComponent implements OnInit {
  readonly categories = signal<StyleCategoryDto[]>([]);
  readonly order = signal<OrderDto | null>(null);
  readonly loading = signal(false);
  readonly error = signal<string | null>(null);
  readonly selectedMonth = signal<number | null>(null);

  readonly months = [
    { number: 1, name: 'Січень' },
    { number: 2, name: 'Лютий' },
    { number: 3, name: 'Березень' },
    { number: 4, name: 'Квітень' },
    { number: 5, name: 'Травень' },
    { number: 6, name: 'Червень' },
    { number: 7, name: 'Липень' },
    { number: 8, name: 'Серпень' },
    { number: 9, name: 'Вересень' },
    { number: 10, name: 'Жовтень' },
    { number: 11, name: 'Листопад' },
    { number: 12, name: 'Грудень' },
  ];

  newDay = 1;
  newLabel = '';

  private readonly orderId: string;

  constructor(
    private readonly route: ActivatedRoute,
    private readonly router: Router,
    private readonly orders: OrderService,
  ) {
    this.orderId = this.route.snapshot.paramMap.get('orderId')!;
  }

  ngOnInit(): void {
    this.orders.styleCategories().subscribe((cats) => this.categories.set(cats));
    this.orders.getOrder(this.orderId).subscribe((o) => this.order.set(o));
  }

  pad(n: number): string {
    return n.toString().padStart(2, '0');
  }

  pickStyle(cat: StyleCategoryDto): void {
    this.orders.selectStyle(this.orderId, cat.id).subscribe((o) => this.order.set(o));
  }

  datesForMonth(month: number) {
    return (this.order()?.personalDates ?? []).filter((d) => d.month === month);
  }

  monthName(month: number): string {
    return this.months.find((m) => m.number === month)?.name ?? '';
  }

  openMonth(month: number): void {
    this.selectedMonth.set(month);
    this.newDay = 1;
    this.newLabel = '';
    this.error.set(null);
  }

  closeModal(): void {
    this.selectedMonth.set(null);
  }

  addDate(): void {
    const month = this.selectedMonth();
    if (!month) return;
    const label = this.newLabel.trim();
    if (!label) return;
    this.orders.addDate(this.orderId, this.newDay, month, label).subscribe({
      next: (o) => {
        this.order.set(o);
        this.newLabel = '';
      },
      error: () => this.error.set('Не вдалося додати дату.'),
    });
  }

  removeDate(dateId: string): void {
    this.orders.removeDate(this.orderId, dateId).subscribe((o) => this.order.set(o));
  }

  startGeneration(): void {
    this.loading.set(true);
    this.orders.startGeneration(this.orderId).subscribe({
      next: () => this.router.navigate(['/order', this.orderId, 'generating']),
      error: () => {
        this.error.set('Не вдалося почати генерацію.');
        this.loading.set(false);
      },
    });
  }
}
