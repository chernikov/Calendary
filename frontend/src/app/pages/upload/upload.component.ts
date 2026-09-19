import { Component, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Router } from '@angular/router';
import { Store } from '@ngrx/store';
import { Actions, ofType } from '@ngrx/effects';
import { Subject, concatMap, take } from 'rxjs';
import { OrderActions, selectOrder, selectOrderBusy, selectOrderError } from '../../core/state/order';

// Mirrors PhotoIntake.MaxBytes on the backend.
const MAX_PHOTO_BYTES = 20 * 1024 * 1024;

@Component({
  selector: 'app-upload',
  standalone: true,
  template: `
    <div class="page page-narrow">
      <div class="step-label"><span>Крок 2 із 5</span></div>
      <h2 style="font-size: 28px;">Ваші фото</h2>
      <p class="text-muted">
        Додайте одне або кілька чітких фото обличчя, без сонцезахисних окулярів чи капелюхів,
        гарне освітлення. Кілька фото додають різноманітності — різні місяці календаря можуть
        спиратися на різні знімки.
      </p>

      @if (order()?.photos?.length) {
        <div style="display: grid; grid-template-columns: repeat(auto-fill, minmax(96px, 1fr)); gap: 8px; margin-bottom: var(--space-3);">
          @for (p of order()!.photos; track p.id) {
            <div style="position: relative;">
              <img
                [src]="p.thumbUrl"
                style="width: 100%; aspect-ratio: 1; object-fit: cover; border-radius: var(--radius-sm);"
                alt="Фото"
              />
              <button
                type="button"
                class="btn btn-ghost"
                style="position: absolute; top: 2px; right: 2px; padding: 2px 8px; line-height: 1; background: var(--color-surface);"
                [disabled]="loading() || order()!.photos.length === 1"
                (click)="removePhoto(p.id)"
              >
                ×
              </button>
            </div>
          }
        </div>
      }

      <label
        for="photo"
        style="display: flex; flex-direction: column; align-items: center; justify-content: center; gap: 8px;
               border: 1px dashed var(--color-divider); border-radius: var(--radius-md); padding: var(--space-6);
               cursor: pointer; background: var(--color-surface); text-align: center;"
      >
        <span class="d" style="font-family: var(--font-heading); font-weight: 600; font-size: 18px;">
          {{ order()?.photos?.length ? 'Додати ще фото' : 'Перетягніть фото сюди' }}
        </span>
        <span class="text-muted" style="font-size: 13px;">або натисніть, щоб обрати файл(и)</span>
      </label>
      <input
        id="photo"
        type="file"
        multiple
        accept="image/*"
        style="display: none;"
        (change)="onFileSelected($event)"
      />

      @if (fileError() || error()) {
        <p style="color: var(--color-accent-2-700); font-size: 13px; margin-top: var(--space-2);">{{ fileError() || error() }}</p>
      }

      <button class="btn btn-primary btn-block" [disabled]="!order()?.photos?.length || loading()" (click)="continue()">
        Далі
      </button>
    </div>
  `,
})
export class UploadComponent {
  readonly fileError = signal<string | null>(null);
  private readonly store = inject(Store);

  readonly order = this.store.selectSignal(selectOrder);
  readonly loading = this.store.selectSignal(selectOrderBusy);
  readonly error = this.store.selectSignal(selectOrderError);

  // Uploads are one file per request (see #347) — queued and drained one at a time, since the
  // very first upload must finish (creating the order) before any later one has an orderId to
  // target, and requests must not race each other.
  private readonly uploadQueue = new Subject<File>();

  constructor(
    private readonly router: Router,
    private readonly actions$: Actions,
  ) {
    this.uploadQueue
      .pipe(
        concatMap((file) => this.uploadOne(file)),
        takeUntilDestroyed(),
      )
      .subscribe();
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const files = input.files ? Array.from(input.files) : [];
    input.value = ''; // allow re-selecting the same file(s) later

    let rejected = false;
    for (const file of files) {
      if (!file.type.startsWith('image/')) {
        rejected = true;
        continue;
      }
      if (file.size > MAX_PHOTO_BYTES) {
        rejected = true;
        continue;
      }
      this.uploadQueue.next(file);
    }

    this.fileError.set(rejected ? 'Деякі файли пропущено — лише зображення до 20 МБ.' : null);
  }

  removePhoto(photoId: string): void {
    const orderId = this.order()?.id;
    if (!orderId) return;
    this.store.dispatch(OrderActions.removeOrderPhoto({ orderId, photoId }));
  }

  continue(): void {
    const order = this.order();
    if (!order?.photos?.length) return;
    this.router.navigate(['/order', order.id, 'style']);
  }

  private uploadOne(file: File) {
    const orderId = this.order()?.id;
    if (!orderId) {
      this.store.dispatch(OrderActions.createOrderWithPhoto({ photo: file }));
      return this.actions$.pipe(
        ofType(OrderActions.createOrderWithPhotoSuccess, OrderActions.createOrderWithPhotoFailure),
        take(1),
      );
    }

    this.store.dispatch(OrderActions.addOrderPhoto({ orderId, photo: file }));
    return this.actions$.pipe(
      ofType(OrderActions.addOrderPhotoSuccess, OrderActions.addOrderPhotoFailure),
      take(1),
    );
  }
}
