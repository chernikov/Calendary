import { Component, OnDestroy, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { Subscription } from 'rxjs';
import { Store } from '@ngrx/store';
import { OrderActions, selectOrder, selectOrderBusy, selectOrderError, selectPromptLibrary } from '../../core/state/order';
import { OrderDto, SheetDto } from '../../core/models';
import { SheetPickerModalComponent } from '../style-dates/sheet-picker-modal.component';

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
              <button class="btn btn-primary" style="align-self: flex-start;" [disabled]="busy()" (click)="openModal()">Спробувати ще раз</button>
            </div>
          } @else if (s.imageUrl) {
            <div
              class="gen-card has-image"
              style="aspect-ratio: 3/4; width: 100%; border-radius: var(--radius-md);"
              [style.background-image]="'url(' + s.imageUrl + ')'"
              (click)="openModal()"
            >
              <div class="gen-card-hover-hint">Змінити…</div>
            </div>
          } @else {
            <div class="sheet-thumb generating" style="aspect-ratio: 3/4; width: 100%;"></div>
            <p class="text-muted" style="font-size: 13px; margin-top: var(--space-2);">Цей аркуш ще генерується…</p>
          }

          @if (personalDatesForMonth(o).length > 0) {
            <p class="text-muted" style="font-size: 12.5px; margin-top: var(--space-2);">
              У сітці цього місяця: {{ personalDatesForMonth(o).join(', ') }}
            </p>
          }

          @if (error()) {
            <p style="color: var(--color-accent-2-700); font-size: 13px;">{{ error() }}</p>
          }

          <div style="display: flex; gap: 10px; margin-top: var(--space-3);">
            <button class="btn btn-primary btn-block" [disabled]="busy()" (click)="next()">
              {{ monthNumber < 12 ? 'Далі' : 'До огляду' }}
            </button>
          </div>

          @if (modalOpen()) {
            <app-sheet-picker-modal
              [sheetName]="monthName"
              [photos]="o.photos"
              [library]="library()"
              [initialPromptId]="s.promptId ?? ''"
              [initialStyleId]="s.imageStyleId ?? ''"
              [initialPhotoId]="s.photoId ?? ''"
              [variants]="s.variants"
              [activeVariantId]="s.activeVariantId"
              [status]="s.status"
              [regenerationsRemaining]="o.regenerationsRemaining"
              [photoUploadError]="error()"
              (closed)="closeModal()"
              (generate)="onGenerate(s, $event)"
              (activateVariant)="onActivateVariant(s, $event)"
              (addPhoto)="onAddPhoto($event)"
            />
          }
        }
      }
    </div>
  `,
  imports: [SheetPickerModalComponent],
})
export class MonthComponent implements OnInit, OnDestroy {
  private readonly store = inject(Store);
  readonly order = this.store.selectSignal(selectOrder);
  readonly library = this.store.selectSignal(selectPromptLibrary);
  readonly busy = this.store.selectSignal(selectOrderBusy);
  readonly error = this.store.selectSignal(selectOrderError);
  readonly modalOpen = signal(false);
  readonly orderId: string;
  monthNumber = 1;
  private paramSub?: Subscription;

  constructor(
    private readonly route: ActivatedRoute,
    private readonly router: Router,
  ) {
    this.orderId = this.route.snapshot.paramMap.get('orderId')!;
    this.monthNumber = Number(this.route.snapshot.paramMap.get('month'));
  }

  get monthName(): string {
    return MONTH_NAMES[this.monthNumber - 1] ?? '';
  }

  ngOnInit(): void {
    this.store.dispatch(OrderActions.loadPromptLibrary());
    // The month route is reused between /months/1 and /months/2 etc, so the component
    // instance survives navigation — re-read the param instead of relying on the constructor.
    this.paramSub = this.route.paramMap.subscribe((pm) => {
      this.monthNumber = Number(pm.get('month'));
      this.modalOpen.set(false);
    });

    this.store.dispatch(OrderActions.startOrderPolling({ orderId: this.orderId, intervalMs: 1500 }));
  }

  ngOnDestroy(): void {
    this.store.dispatch(OrderActions.stopOrderPolling());
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

  openModal(): void {
    this.modalOpen.set(true);
  }

  closeModal(): void {
    this.modalOpen.set(false);
  }

  onGenerate(s: SheetDto, picks: { promptId: string; styleId: string; photoId: string }): void {
    this.store.dispatch(
      OrderActions.generateSheet({
        orderId: this.orderId,
        index: s.index,
        promptId: picks.promptId,
        imageStyleId: picks.styleId,
        photoId: picks.photoId || undefined,
      }),
    );
  }

  onActivateVariant(s: SheetDto, variantId: string): void {
    this.store.dispatch(OrderActions.activateVariant({ orderId: this.orderId, sheetId: s.id, variantId }));
  }

  onAddPhoto(photo: File): void {
    this.store.dispatch(OrderActions.addOrderPhoto({ orderId: this.orderId, photo }));
  }

  next(): void {
    if (this.monthNumber < 12) {
      this.router.navigate(['/order', this.orderId, 'months', this.monthNumber + 1]);
    } else {
      this.router.navigate(['/order', this.orderId, 'review']);
    }
  }
}
