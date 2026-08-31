import { Component, OnDestroy, OnInit, effect, inject, signal } from '@angular/core';
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
  selectPromptLibrary,
} from '../../core/state/order';
import { ImageStyleDto, PromptDto, SheetDto, SheetPlanItem } from '../../core/models';

interface PlanRow {
  promptId: string;
  styleId: string;
  /** True once the user explicitly picked a style here — stops downward propagation. */
  styleTouched: boolean;
}

@Component({
  selector: 'app-style-dates',
  standalone: true,
  imports: [FormsModule],
  template: `
    <div class="page">
      <div class="step-label"><span>Крок 3 із 5</span></div>
      <h2 style="font-size: 28px;">Образи</h2>
      <p class="text-muted">
        Оберіть образ (сюжет) і стиль для обкладинки та кожного місяця. Обраний стиль
        застосовується й до наступних аркушів — за бажанням змініть його на будь-якому.
      </p>

      <div class="gen-cover">
        <div class="gen-card" [class.has-image]="imageFor(0)" [style.background-image]="bgFor(0)">
          <div class="gen-card-name">Обкладинка</div>
          <div class="gen-card-controls">
            <button
              type="button"
              class="badge-btn"
              [class.filled]="plan[0].promptId"
              (click)="openPicker(0, 'prompt')"
            >
              {{ promptName(plan[0].promptId) || 'Образ…' }}
            </button>
            <button
              type="button"
              class="badge-btn"
              [class.filled]="plan[0].styleId"
              (click)="openPicker(0, 'style')"
            >
              {{ styleName(plan[0].styleId) || 'Стиль…' }}
            </button>
            @if (isGenerating(0)) {
              <div class="gen-card-status">Генерується…</div>
            } @else if (plan[0].promptId && plan[0].styleId) {
              <button type="button" class="btn btn-primary btn-gen" (click)="generateCard(0)">
                {{ imageFor(0) ? 'Перегенерувати' : 'Згенерувати' }}
              </button>
            }
            @if (isFailed(0)) {
              <div class="gen-card-error">Не вдалося — спробуйте ще раз</div>
            }
          </div>
        </div>
      </div>

      <div class="gen-grid">
        @for (row of monthRows; track row.index) {
          <div class="gen-card" [class.has-image]="imageFor(row.index)" [style.background-image]="bgFor(row.index)">
            <div class="gen-card-name">{{ row.name }}</div>
            <div class="gen-card-controls">
              <button
                type="button"
                class="badge-btn"
                [class.filled]="plan[row.index].promptId"
                (click)="openPicker(row.index, 'prompt')"
              >
                {{ promptName(plan[row.index].promptId) || 'Образ…' }}
              </button>
              <button
                type="button"
                class="badge-btn"
                [class.filled]="plan[row.index].styleId"
                (click)="openPicker(row.index, 'style')"
              >
                {{ styleName(plan[row.index].styleId) || 'Стиль…' }}
              </button>
              @if (isGenerating(row.index)) {
                <div class="gen-card-status">Генерується…</div>
              } @else if (plan[row.index].promptId && plan[row.index].styleId) {
                <button type="button" class="btn btn-primary btn-gen" (click)="generateCard(row.index)">
                  {{ imageFor(row.index) ? 'Перегенерувати' : 'Згенерувати' }}
                </button>
              }
              @if (isFailed(row.index)) {
                <div class="gen-card-error">Не вдалося — спробуйте ще раз</div>
              }
            </div>
          </div>
        }
      </div>

      @if (picker(); as p) {
        <div class="dialog-backdrop" (click)="closePicker()">
          <div class="dialog picker-dialog" (click)="$event.stopPropagation()">
            <div class="dialog-title">
              {{ p.kind === 'prompt' ? 'Образ' : 'Стиль' }} — {{ sheetRows[p.index].name }}
            </div>

            @if (p.kind === 'prompt') {
              @for (theme of library()?.themes ?? []; track theme.id) {
                <div>
                  <div class="card-title" style="margin-bottom: 4px;">{{ theme.name }}</div>
                  <p class="picker-theme-desc">{{ theme.description }}</p>
                  <div class="picker-grid">
                    @for (prompt of theme.prompts; track prompt.id) {
                      <button
                        type="button"
                        class="picker-item"
                        [class.selected]="plan[p.index].promptId === prompt.id"
                        (click)="choosePrompt(prompt.id)"
                      >
                        <img [src]="promptImage(prompt)" [alt]="prompt.name" loading="lazy" />
                        <span class="picker-item-name">{{ prompt.name }}</span>
                        <span class="picker-item-desc">{{ prompt.description }}</span>
                      </button>
                    }
                  </div>
                </div>
              }
            } @else {
              <div class="picker-grid">
                @for (style of library()?.styles ?? []; track style.id) {
                  <button
                    type="button"
                    class="picker-item"
                    [class.selected]="plan[p.index].styleId === style.id"
                    (click)="chooseStyle(style.id)"
                  >
                    <img [src]="styleImage(style)" [alt]="style.name" loading="lazy" />
                    <span class="picker-item-name">{{ style.name }}</span>
                    <span class="picker-item-desc">{{ style.description }}</span>
                  </button>
                }
              </div>
            }

            <div class="dialog-actions">
              <button class="btn btn-secondary" (click)="closePicker()">Закрити</button>
            </div>
          </div>
        </div>
      }

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
        [disabled]="!planComplete() || loading()"
        (click)="startGeneration()"
      >
        Почати генерацію
      </button>
    </div>
  `,
})
export class StyleDatesComponent implements OnInit, OnDestroy {
  private readonly store = inject(Store);
  readonly library = this.store.selectSignal(selectPromptLibrary);
  readonly order = this.store.selectSignal(selectOrder);
  readonly loading = this.store.selectSignal(selectOrderBusy);
  readonly error = this.store.selectSignal(selectOrderError);
  readonly selectedMonth = signal<number | null>(null);
  readonly picker = signal<{ index: number; kind: 'prompt' | 'style' } | null>(null);

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

  // Index 0 = cover, 1..12 = months.
  readonly sheetRows = [
    { index: 0, name: 'Обкладинка' },
    ...Array.from({ length: 12 }, (_, i) => ({ index: i + 1, name: this.monthNameByNumber(i + 1) })),
  ];
  readonly monthRows = this.sheetRows.slice(1);
  readonly plan: PlanRow[] = this.sheetRows.map(() => ({ promptId: '', styleId: '', styleTouched: false }));
  private planHydrated = false;

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

    // A previously saved plan comes back on the order's sheets — hydrate the selects once.
    effect(() => {
      const sheets = this.order()?.sheets ?? [];
      if (this.planHydrated || sheets.length === 0) return;
      this.planHydrated = true;
      for (const sheet of sheets) {
        const row = this.plan[sheet.index];
        if (!row) continue;
        row.promptId = sheet.promptId ?? '';
        row.styleId = sheet.imageStyleId ?? '';
        row.styleTouched = !!sheet.imageStyleId;
      }
    });
  }

  ngOnInit(): void {
    this.store.dispatch(OrderActions.loadPromptLibrary());
    // Poll while on this page — per-card generation completes in the background.
    this.store.dispatch(OrderActions.startOrderPolling({ orderId: this.orderId, intervalMs: 2000 }));
  }

  ngOnDestroy(): void {
    this.store.dispatch(OrderActions.stopOrderPolling());
  }

  private sheetFor(index: number): SheetDto | undefined {
    return this.order()?.sheets.find((s) => s.index === index);
  }

  imageFor(index: number): string | null {
    return this.sheetFor(index)?.imageUrl ?? null;
  }

  bgFor(index: number): string | null {
    const url = this.imageFor(index);
    return url ? `url('${url}')` : null;
  }

  isGenerating(index: number): boolean {
    return this.sheetFor(index)?.status === 'Generating';
  }

  isFailed(index: number): boolean {
    return this.sheetFor(index)?.status === 'Failed';
  }

  generateCard(index: number): void {
    const row = this.plan[index];
    if (!row.promptId || !row.styleId || this.isGenerating(index)) return;
    this.store.dispatch(
      OrderActions.generateSheet({
        orderId: this.orderId,
        index,
        promptId: row.promptId,
        imageStyleId: row.styleId,
      }),
    );
  }

  private monthNameByNumber(month: number): string {
    return [
      'Січень', 'Лютий', 'Березень', 'Квітень', 'Травень', 'Червень',
      'Липень', 'Серпень', 'Вересень', 'Жовтень', 'Листопад', 'Грудень',
    ][month - 1];
  }

  pad(n: number): string {
    return n.toString().padStart(2, '0');
  }

  pickPrompt(index: number, promptId: string): void {
    this.plan[index].promptId = promptId;
  }

  openPicker(index: number, kind: 'prompt' | 'style'): void {
    this.picker.set({ index, kind });
  }

  closePicker(): void {
    this.picker.set(null);
  }

  choosePrompt(promptId: string): void {
    const p = this.picker();
    if (!p) return;
    this.pickPrompt(p.index, promptId);
    this.closePicker();
  }

  chooseStyle(styleId: string): void {
    const p = this.picker();
    if (!p) return;
    this.pickStyle(p.index, styleId);
    this.closePicker();
  }

  promptName(promptId: string): string {
    if (!promptId) return '';
    for (const theme of this.library()?.themes ?? []) {
      const prompt = theme.prompts.find((x) => x.id === promptId);
      if (prompt) return prompt.name;
    }
    return '';
  }

  styleName(styleId: string): string {
    return this.library()?.styles.find((s) => s.id === styleId)?.name ?? '';
  }

  // Admin-generated previews land in previewImageUrl; until then show a stable placeholder.
  promptImage(prompt: PromptDto): string {
    return prompt.previewImageUrl ?? `https://picsum.photos/seed/prompt-${prompt.id}/240/300`;
  }

  styleImage(style: ImageStyleDto): string {
    return style.previewImageUrl ?? `https://picsum.photos/seed/style-${style.id}/240/300`;
  }

  // The chosen style “sticks”: it flows down to every later sheet the user hasn't overridden.
  pickStyle(index: number, styleId: string): void {
    this.plan[index].styleId = styleId;
    this.plan[index].styleTouched = true;
    for (let i = index + 1; i < this.plan.length; i++) {
      if (!this.plan[i].styleTouched) {
        this.plan[i].styleId = styleId;
      }
    }
  }

  planComplete(): boolean {
    return this.plan.every((row) => row.promptId && row.styleId);
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
    if (!this.planComplete()) return;
    const items: SheetPlanItem[] = this.plan.map((row, index) => ({
      index,
      promptId: row.promptId,
      imageStyleId: row.styleId,
    }));
    this.store.dispatch(OrderActions.savePlanAndGenerate({ orderId: this.orderId, items }));
  }
}
