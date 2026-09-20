import { Component, OnDestroy, OnInit, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ActivatedRoute, Router } from '@angular/router';
import { Store } from '@ngrx/store';
import { Actions, ofType } from '@ngrx/effects';
import {
  OrderActions,
  selectCoverSheet,
  selectOrder,
  selectOrderBusy,
  selectOrderError,
  selectPromptLibrary,
} from '../../core/state/order';
import { SheetPickerModalComponent } from '../style-dates/sheet-picker-modal.component';

@Component({
  selector: 'app-cover',
  standalone: true,
  template: `
    <div class="page page-narrow">
      <div class="step-label"><span>Крок 5 із 5</span></div>
      <h2 style="font-size: 28px;">Обкладинка</h2>
      <p class="text-muted">Вона задає стиль усіх дванадцяти місяців. Після підтвердження змінити її не вийде.</p>

      @if (cover(); as c) {
        @if (c.status === 'Failed') {
          <div class="card" style="border: 1px solid var(--color-accent-2-300); background: var(--color-accent-2-100);">
            <div class="card-title">Не вдалося згенерувати обкладинку</div>
            <p class="card-body">{{ c.failureReason || 'Спробуйте ще раз.' }}</p>
            <button class="btn btn-primary" style="align-self: flex-start;" [disabled]="busy()" (click)="openModal()">
              Спробувати ще раз
            </button>
          </div>
        } @else if (c.imageUrl) {
          <div
            class="gen-card has-image"
            style="aspect-ratio: 3/4; width: 100%; border-radius: var(--radius-md); border: 1.5px solid var(--color-accent);"
            [style.background-image]="'url(' + c.imageUrl + ')'"
            (click)="openModal()"
          >
            <div class="gen-card-hover-hint">Змінити…</div>
          </div>
        } @else {
          <div class="sheet-thumb generating" style="aspect-ratio: 3/4; width: 100%;"></div>
          <p class="text-muted" style="font-size: 13px; margin-top: var(--space-2);">Обкладинка ще генерується…</p>
        }

        @if (error()) {
          <p style="color: var(--color-accent-2-700); font-size: 13px;">{{ error() }}</p>
        }

        <div style="display: flex; gap: 10px; margin-top: var(--space-3);">
          <button class="btn btn-primary btn-block" [disabled]="!c.imageUrl || busy()" (click)="confirm()">
            Обрати цю
          </button>
        </div>

        @if (modalOpen()) {
          <app-sheet-picker-modal
            sheetName="Обкладинка"
            [photos]="order()?.photos ?? []"
            [library]="library()"
            [initialPromptId]="c.promptId ?? ''"
            [initialStyleId]="c.imageStyleId ?? ''"
            [initialPhotoId]="c.photoId ?? ''"
            [variants]="c.variants"
            [activeVariantId]="c.activeVariantId"
            [status]="c.status"
            [regenerationsRemaining]="order()?.regenerationsRemaining ?? 0"
            [photoUploadError]="error()"
            (closed)="closeModal()"
            (generate)="onGenerate($event)"
            (activateVariant)="onActivateVariant($event)"
            (addPhoto)="onAddPhoto($event)"
          />
        }
      }
    </div>
  `,
  imports: [SheetPickerModalComponent],
})
export class CoverComponent implements OnInit, OnDestroy {
  private readonly store = inject(Store);
  readonly order = this.store.selectSignal(selectOrder);
  readonly cover = this.store.selectSignal(selectCoverSheet);
  readonly library = this.store.selectSignal(selectPromptLibrary);
  readonly busy = this.store.selectSignal(selectOrderBusy);
  readonly error = this.store.selectSignal(selectOrderError);
  readonly modalOpen = signal(false);
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
    this.store.dispatch(OrderActions.loadPromptLibrary());
    this.store.dispatch(OrderActions.startOrderPolling({ orderId: this.orderId, intervalMs: 1500 }));
  }

  ngOnDestroy(): void {
    this.store.dispatch(OrderActions.stopOrderPolling());
  }

  openModal(): void {
    this.modalOpen.set(true);
  }

  closeModal(): void {
    this.modalOpen.set(false);
  }

  onGenerate(picks: { promptId: string; styleId: string; photoId: string }): void {
    const c = this.cover();
    if (!c) return;
    this.store.dispatch(
      OrderActions.generateSheet({
        orderId: this.orderId,
        index: c.index,
        promptId: picks.promptId,
        imageStyleId: picks.styleId,
        photoId: picks.photoId || undefined,
      }),
    );
  }

  onActivateVariant(variantId: string): void {
    const c = this.cover();
    if (!c) return;
    this.store.dispatch(OrderActions.activateVariant({ orderId: this.orderId, sheetId: c.id, variantId }));
  }

  onAddPhoto(photo: File): void {
    this.store.dispatch(OrderActions.addOrderPhoto({ orderId: this.orderId, photo }));
  }

  confirm(): void {
    const c = this.cover();
    if (!c) return;
    this.store.dispatch(OrderActions.confirmCover({ orderId: this.orderId, sheetId: c.id }));
  }
}
