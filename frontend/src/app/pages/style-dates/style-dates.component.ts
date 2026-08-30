import { Component, OnInit, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { Store } from '@ngrx/store';
import { Actions, ofType } from '@ngrx/effects';
import {
  OrderActions,
  selectOrder,
  selectOrderBusy,
  selectOrderError,
  selectStyleCategories,
} from '../../core/state/order';
import { StyleCategoryDto } from '../../core/models';

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
            <div class="tile-calendar">
              @for (day of calendarCells(m.number); track $index) {
                @if (day === null) {
                  <span class="tile-calendar-day empty"></span>
                } @else {
                  <span
                    class="tile-calendar-day"
                    [class.has-date]="hasDate(m.number, day)"
                    [title]="labelForDay(m.number, day)"
                  >
                    {{ day }}
                  </span>
                }
              }
            </div>
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

            <div class="calendar-grid">
              @for (w of weekdays; track w) {
                <div class="calendar-weekday">{{ w }}</div>
              }
              @for (day of calendarCells(month); track $index) {
                @if (day === null) {
                  <div class="calendar-day empty"></div>
                } @else {
                  <button
                    type="button"
                    class="calendar-day"
                    [class.has-date]="hasDate(month, day)"
                    [class.selected]="newDay === day"
                    (click)="selectDay(day)"
                  >
                    {{ day }}
                  </button>
                }
              }
            </div>

            <div class="field">
              <label>Підпис (до 22 символів)</label>
              <input class="input" maxlength="22" [(ngModel)]="newLabel" />
            </div>

            @if (error()) {
              <p style="color: var(--color-accent-2-700); font-size: 13px;">{{ error() }}</p>
            }

            <div class="dialog-actions">
              <button class="btn btn-secondary" (click)="closeModal()">Готово</button>
              <button class="btn btn-primary" [disabled]="!newLabel || !newDay" (click)="addDate()">Додати дату</button>
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
  private readonly store = inject(Store);
  readonly categories = this.store.selectSignal(selectStyleCategories);
  readonly order = this.store.selectSignal(selectOrder);
  readonly loading = this.store.selectSignal(selectOrderBusy);
  readonly error = this.store.selectSignal(selectOrderError);
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

  readonly weekdays = ['Пн', 'Вт', 'Ср', 'Чт', 'Пт', 'Сб', 'Нд'];
  private readonly calendarYear = new Date().getFullYear() + 1;

  newDay: number | null = null;
  newLabel = '';

  private readonly orderId: string;

  constructor(
    private readonly route: ActivatedRoute,
    private readonly router: Router,
    private readonly actions$: Actions,
  ) {
    this.orderId = this.route.snapshot.paramMap.get('orderId')!;
    this.actions$
      .pipe(ofType(OrderActions.startGenerationSuccess), takeUntilDestroyed())
      .subscribe(() => this.router.navigate(['/order', this.orderId, 'generating']));
    this.actions$
      .pipe(ofType(OrderActions.addPersonalDateSuccess), takeUntilDestroyed())
      .subscribe(() => (this.newLabel = ''));
  }

  ngOnInit(): void {
    this.store.dispatch(OrderActions.loadStyleCategories());
    this.store.dispatch(OrderActions.loadOrder({ orderId: this.orderId }));
  }

  pad(n: number): string {
    return n.toString().padStart(2, '0');
  }

  pickStyle(cat: StyleCategoryDto): void {
    this.store.dispatch(OrderActions.selectStyle({ orderId: this.orderId, styleCategoryId: cat.id }));
  }

  datesForMonth(month: number) {
    return (this.order()?.personalDates ?? []).filter((d) => d.month === month);
  }

  monthName(month: number): string {
    return this.months.find((m) => m.number === month)?.name ?? '';
  }

  calendarCells(month: number): (number | null)[] {
    const firstWeekday = (new Date(this.calendarYear, month - 1, 1).getDay() + 6) % 7;
    const daysInMonth = new Date(this.calendarYear, month, 0).getDate();
    const cells: (number | null)[] = new Array(firstWeekday).fill(null);
    for (let day = 1; day <= daysInMonth; day++) cells.push(day);
    while (cells.length < 42) cells.push(null);
    return cells;
  }

  hasDate(month: number, day: number): boolean {
    return this.datesForMonth(month).some((d) => d.day === day);
  }

  labelForDay(month: number, day: number): string {
    return this.datesForMonth(month)
      .filter((d) => d.day === day)
      .map((d) => d.label)
      .join(', ');
  }

  selectDay(day: number): void {
    this.newDay = day;
  }

  openMonth(month: number): void {
    this.selectedMonth.set(month);
    this.newDay = null;
    this.newLabel = '';
    this.store.dispatch(OrderActions.clearOrderError());
  }

  closeModal(): void {
    this.selectedMonth.set(null);
  }

  addDate(): void {
    const month = this.selectedMonth();
    const day = this.newDay;
    if (!month || !day) return;
    const label = this.newLabel.trim();
    if (!label) return;
    this.store.dispatch(OrderActions.addPersonalDate({ orderId: this.orderId, day, month, label }));
  }

  removeDate(dateId: string): void {
    this.store.dispatch(OrderActions.removePersonalDate({ orderId: this.orderId, dateId }));
  }

  startGeneration(): void {
    this.store.dispatch(OrderActions.startGeneration({ orderId: this.orderId }));
  }
}
