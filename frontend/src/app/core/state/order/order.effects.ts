import { Injectable, inject } from '@angular/core';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { catchError, interval, map, of, startWith, switchMap, takeUntil, tap } from 'rxjs';
import { OrderService } from '../../order.service';
import { OrderActions } from './order.actions';

@Injectable()
export class OrderEffects {
  private readonly actions$ = inject(Actions);
  private readonly orders = inject(OrderService);

  loadOrder$ = createEffect(() =>
    this.actions$.pipe(
      ofType(OrderActions.loadOrder),
      switchMap(({ orderId }) =>
        this.orders.getOrder(orderId).pipe(
          map((order) => OrderActions.loadOrderSuccess({ order })),
          catchError(() => of(OrderActions.loadOrderFailure({ error: 'Не вдалося завантажити замовлення.' }))),
        ),
      ),
    ),
  );

  startOrderPolling$ = createEffect(() =>
    this.actions$.pipe(
      ofType(OrderActions.startOrderPolling),
      switchMap(({ orderId, intervalMs }) =>
        interval(intervalMs).pipe(
          startWith(0),
          switchMap(() =>
            this.orders.getOrder(orderId).pipe(
              map((order) => OrderActions.loadOrderSuccess({ order })),
              catchError(() => of(OrderActions.loadOrderFailure({ error: 'Не вдалося оновити замовлення.' }))),
            ),
          ),
          takeUntil(this.actions$.pipe(ofType(OrderActions.stopOrderPolling))),
        ),
      ),
    ),
  );

  loadStyleCategories$ = createEffect(() =>
    this.actions$.pipe(
      ofType(OrderActions.loadStyleCategories),
      switchMap(() =>
        this.orders.styleCategories().pipe(
          map((categories) => OrderActions.loadStyleCategoriesSuccess({ categories })),
          catchError(() => of(OrderActions.loadStyleCategoriesFailure({ error: 'Не вдалося завантажити стилі.' }))),
        ),
      ),
    ),
  );

  uploadPhoto$ = createEffect(() =>
    this.actions$.pipe(
      ofType(OrderActions.uploadPhoto),
      switchMap(({ orderId, photoDataUrl }) =>
        this.orders.uploadPhoto(orderId, photoDataUrl).pipe(
          map((order) => OrderActions.uploadPhotoSuccess({ order })),
          catchError(() => of(OrderActions.uploadPhotoFailure({ error: 'Не вдалося завантажити фото.' }))),
        ),
      ),
    ),
  );

  selectStyle$ = createEffect(() =>
    this.actions$.pipe(
      ofType(OrderActions.selectStyle),
      switchMap(({ orderId, styleCategoryId }) =>
        this.orders.selectStyle(orderId, styleCategoryId).pipe(
          map((order) => OrderActions.selectStyleSuccess({ order })),
          catchError(() => of(OrderActions.selectStyleFailure({ error: 'Не вдалося обрати стиль.' }))),
        ),
      ),
    ),
  );

  addPersonalDate$ = createEffect(() =>
    this.actions$.pipe(
      ofType(OrderActions.addPersonalDate),
      switchMap(({ orderId, day, month, label }) =>
        this.orders.addDate(orderId, day, month, label).pipe(
          map((order) => OrderActions.addPersonalDateSuccess({ order })),
          catchError(() => of(OrderActions.addPersonalDateFailure({ error: 'Не вдалося додати дату.' }))),
        ),
      ),
    ),
  );

  removePersonalDate$ = createEffect(() =>
    this.actions$.pipe(
      ofType(OrderActions.removePersonalDate),
      switchMap(({ orderId, dateId }) =>
        this.orders.removeDate(orderId, dateId).pipe(
          map((order) => OrderActions.removePersonalDateSuccess({ order })),
          catchError(() => of(OrderActions.removePersonalDateFailure({ error: 'Не вдалося видалити дату.' }))),
        ),
      ),
    ),
  );

  startGeneration$ = createEffect(() =>
    this.actions$.pipe(
      ofType(OrderActions.startGeneration),
      switchMap(({ orderId }) =>
        this.orders.startGeneration(orderId).pipe(
          map((order) => OrderActions.startGenerationSuccess({ order })),
          catchError(() => of(OrderActions.startGenerationFailure({ error: 'Не вдалося почати генерацію.' }))),
        ),
      ),
    ),
  );

  regenerateSheet$ = createEffect(() =>
    this.actions$.pipe(
      ofType(OrderActions.regenerateSheet),
      switchMap(({ orderId, sheetId }) =>
        this.orders.regenerateSheet(orderId, sheetId).pipe(
          map((order) => OrderActions.regenerateSheetSuccess({ order })),
          catchError(() => of(OrderActions.regenerateSheetFailure({ error: 'Перегенерації вичерпано.' }))),
        ),
      ),
    ),
  );

  confirmCover$ = createEffect(() =>
    this.actions$.pipe(
      ofType(OrderActions.confirmCover),
      switchMap(({ orderId, sheetId }) =>
        this.orders.confirmCover(orderId, sheetId).pipe(
          map((order) => OrderActions.confirmCoverSuccess({ order })),
          catchError(() => of(OrderActions.confirmCoverFailure({ error: 'Не вдалося підтвердити обкладинку.' }))),
        ),
      ),
    ),
  );

  loadWarehouses$ = createEffect(() =>
    this.actions$.pipe(
      ofType(OrderActions.loadWarehouses),
      switchMap(({ city }) =>
        this.orders.novaPoshtaWarehouses(city).pipe(
          map((warehouses) => OrderActions.loadWarehousesSuccess({ warehouses })),
          catchError(() => of(OrderActions.loadWarehousesFailure({ error: 'Не вдалося завантажити відділення.' }))),
        ),
      ),
    ),
  );

  checkoutAndPay$ = createEffect(() =>
    this.actions$.pipe(
      ofType(OrderActions.checkoutAndPay),
      switchMap(({ orderId, delivery, method }) =>
        this.orders.checkout(orderId, delivery).pipe(
          catchError(() => {
            throw { step: 'checkout' as const };
          }),
          switchMap(() =>
            this.orders.pay(orderId, method).pipe(
              catchError(() => {
                throw { step: 'pay' as const };
              }),
            ),
          ),
          map((order) => OrderActions.checkoutAndPaySuccess({ order })),
          catchError((err: { step?: 'checkout' | 'pay' }) =>
            of(
              OrderActions.checkoutAndPayFailure({
                error:
                  err?.step === 'pay'
                    ? 'Оплата не пройшла. Спробуйте ще раз.'
                    : 'Не вдалося зберегти дані доставки.',
              }),
            ),
          ),
        ),
      ),
    ),
  );

  cancelOrder$ = createEffect(() =>
    this.actions$.pipe(
      ofType(OrderActions.cancelOrder),
      switchMap(({ orderId }) =>
        this.orders.cancel(orderId).pipe(
          map((order) => OrderActions.cancelOrderSuccess({ order })),
          catchError(() => of(OrderActions.cancelOrderFailure({ error: 'Не вдалося скасувати замовлення.' }))),
        ),
      ),
    ),
  );

  downloadPdf$ = createEffect(() =>
    this.actions$.pipe(
      ofType(OrderActions.downloadPdf),
      switchMap(({ orderId }) =>
        this.orders.downloadPdf(orderId).pipe(
          tap((blob) => this.triggerBrowserDownload(blob, `calendary-${orderId}.pdf`)),
          map(() => OrderActions.downloadPdfSuccess()),
          catchError(() => of(OrderActions.downloadPdfFailure({ error: 'Не вдалося сформувати PDF.' }))),
        ),
      ),
    ),
  );

  private triggerBrowserDownload(blob: Blob, fileName: string): void {
    const url = URL.createObjectURL(blob);
    const anchor = document.createElement('a');
    anchor.href = url;
    anchor.download = fileName;
    anchor.click();
    URL.revokeObjectURL(url);
  }
}
