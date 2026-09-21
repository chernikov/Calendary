import { HttpErrorResponse } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { Store } from '@ngrx/store';
import { catchError, interval, map, mergeMap, of, startWith, switchMap, takeUntil, tap, withLatestFrom } from 'rxjs';
import { OrderDto, OrderProgressDto } from '../../models';
import { OrderService } from '../../order.service';
import { photoUploadErrorMessage } from '../../photo-upload-error';
import { OrderActions } from './order.actions';
import { selectOrder } from './order.selectors';

// This same polling action/effect is shared by every page that watches order progress
// (generating/month/cover/style-dates during generation, status during fulfillment) — so "nothing
// worth a full reload happened" has to mean *no* status change anywhere: not just a sheet
// finishing (new variant/ImageUrl), but also the order's own status moving (e.g. Paid -> Shipped,
// which is also when Delivery.trackingNumber gets populated — data the lightweight poll doesn't
// carry either). A missing order/id mismatch means the store doesn't hold this order yet at all,
// so it needs a full load regardless. Ticks where literally nothing changed are the common case
// while generation/fulfillment is mid-flight, and are the ones this saves from re-fetching the
// full ImageUrl-laden order every 1.5-2s.
function needsFullOrderReload(current: OrderDto | null, orderId: string, progress: OrderProgressDto): boolean {
  if (!current || current.id !== orderId) return true;
  if (current.status !== progress.status) return true;
  return progress.sheets.some((p) => {
    const existing = current.sheets.find((s) => s.kind === p.kind && s.index === p.index);
    return !existing || existing.status !== p.status;
  });
}

@Injectable()
export class OrderEffects {
  private readonly actions$ = inject(Actions);
  private readonly orders = inject(OrderService);
  private readonly store = inject(Store);

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

  loadMyOrders$ = createEffect(() =>
    this.actions$.pipe(
      ofType(OrderActions.loadMyOrders),
      switchMap(() =>
        this.orders.listOrders().pipe(
          map((orders) => OrderActions.loadMyOrdersSuccess({ orders })),
          catchError(() => of(OrderActions.loadMyOrdersFailure({ error: 'Не вдалося завантажити замовлення.' }))),
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
          withLatestFrom(this.store.select(selectOrder)),
          switchMap(([, currentOrder]) =>
            this.orders.getOrderProgress(orderId).pipe(
              map((progress) =>
                needsFullOrderReload(currentOrder, orderId, progress)
                  ? OrderActions.loadOrder({ orderId })
                  : OrderActions.orderProgressSuccess({ progress }),
              ),
              catchError(() => of(OrderActions.loadOrderFailure({ error: 'Не вдалося оновити замовлення.' }))),
            ),
          ),
          takeUntil(this.actions$.pipe(ofType(OrderActions.stopOrderPolling))),
        ),
      ),
    ),
  );

  loadPromptLibrary$ = createEffect(() =>
    this.actions$.pipe(
      ofType(OrderActions.loadPromptLibrary),
      switchMap(() =>
        this.orders.promptLibrary().pipe(
          map((library) => OrderActions.loadPromptLibrarySuccess({ library })),
          catchError(() => of(OrderActions.loadPromptLibraryFailure({ error: 'Не вдалося завантажити бібліотеку промптів.' }))),
        ),
      ),
    ),
  );

  loadHolidays$ = createEffect(() =>
    this.actions$.pipe(
      ofType(OrderActions.loadHolidays),
      switchMap(({ year }) =>
        this.orders.listHolidays(year).pipe(
          map((holidays) => OrderActions.loadHolidaysSuccess({ holidays })),
          catchError(() => of(OrderActions.loadHolidaysFailure({ error: 'Не вдалося завантажити свята.' }))),
        ),
      ),
    ),
  );

  createOrderWithPhoto$ = createEffect(() =>
    this.actions$.pipe(
      ofType(OrderActions.createOrderWithPhoto),
      switchMap(({ photo }) =>
        this.orders.createOrder(photo).pipe(
          map((order) => OrderActions.createOrderWithPhotoSuccess({ order })),
          catchError((err) => of(OrderActions.createOrderWithPhotoFailure({ error: photoUploadErrorMessage(err) }))),
        ),
      ),
    ),
  );

  addOrderPhoto$ = createEffect(() =>
    this.actions$.pipe(
      ofType(OrderActions.addOrderPhoto),
      switchMap(({ orderId, photo }) =>
        this.orders.addPhoto(orderId, photo).pipe(
          map((order) => OrderActions.addOrderPhotoSuccess({ order })),
          catchError((err) => of(OrderActions.addOrderPhotoFailure({ error: photoUploadErrorMessage(err) }))),
        ),
      ),
    ),
  );

  removeOrderPhoto$ = createEffect(() =>
    this.actions$.pipe(
      ofType(OrderActions.removeOrderPhoto),
      switchMap(({ orderId, photoId }) =>
        this.orders.removePhoto(orderId, photoId).pipe(
          map((order) => OrderActions.removeOrderPhotoSuccess({ order })),
          catchError(() => of(OrderActions.removeOrderPhotoFailure({ error: 'Не вдалося видалити фото.' }))),
        ),
      ),
    ),
  );

  // Saving the sheet plan and starting generation are one user gesture — chain the two calls
  // and reuse startGenerationSuccess so navigation logic stays in one place.
  savePlanAndGenerate$ = createEffect(() =>
    this.actions$.pipe(
      ofType(OrderActions.savePlanAndGenerate),
      switchMap(({ orderId, items }) =>
        this.orders.saveSheetPlan(orderId, items).pipe(
          switchMap(() =>
            this.orders.startGeneration(orderId).pipe(
              map((order) => OrderActions.startGenerationSuccess({ order })),
            ),
          ),
          catchError(() =>
            of(OrderActions.savePlanAndGenerateFailure({ error: 'Не вдалося почати генерацію.' })),
          ),
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

  generateSheet$ = createEffect(() =>
    this.actions$.pipe(
      ofType(OrderActions.generateSheet),
      // mergeMap: the user may fire several cards in quick succession.
      mergeMap(({ orderId, index, promptId, imageStyleId, photoId }) =>
        this.orders.generateSheet(orderId, index, promptId, imageStyleId, photoId).pipe(
          map((order) => OrderActions.generateSheetSuccess({ order })),
          catchError(() => of(OrderActions.generateSheetFailure({ error: 'Не вдалося згенерувати зображення. Можливо, перегенерації вичерпано.' }))),
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

  saveHolidaySettings$ = createEffect(() =>
    this.actions$.pipe(
      ofType(OrderActions.saveHolidaySettings),
      switchMap(({ orderId, countries, weekStart }) =>
        this.orders.saveHolidaySettings(orderId, countries, weekStart).pipe(
          map((order) => OrderActions.saveHolidaySettingsSuccess({ order })),
          catchError(() => of(OrderActions.saveHolidaySettingsFailure({ error: 'Не вдалося зберегти налаштування свят.' }))),
        ),
      ),
    ),
  );

  applyPromoCode$ = createEffect(() =>
    this.actions$.pipe(
      ofType(OrderActions.applyPromoCode),
      switchMap(({ orderId, code }) =>
        this.orders.applyPromoCode(orderId, code).pipe(
          map((order) => OrderActions.applyPromoCodeSuccess({ order })),
          // The backend returns a specific, user-facing reason (code not found/expired/limit
          // reached/min order amount) as the plain-text/JSON-string error body — surface it as-is
          // rather than a generic message, same idea as #401's generation-failure reasons.
          catchError((err: HttpErrorResponse) =>
            of(OrderActions.applyPromoCodeFailure({
              error: typeof err.error === 'string' ? err.error : 'Не вдалося застосувати промокод.',
            })),
          ),
        ),
      ),
    ),
  );

  removePromoCode$ = createEffect(() =>
    this.actions$.pipe(
      ofType(OrderActions.removePromoCode),
      switchMap(({ orderId }) =>
        this.orders.removePromoCode(orderId).pipe(
          map((order) => OrderActions.removePromoCodeSuccess({ order })),
          catchError(() => of(OrderActions.removePromoCodeFailure({ error: 'Не вдалося прибрати промокод.' }))),
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

  activateVariant$ = createEffect(() =>
    this.actions$.pipe(
      ofType(OrderActions.activateVariant),
      switchMap(({ orderId, sheetId, variantId }) =>
        this.orders.activateVariant(orderId, sheetId, variantId).pipe(
          map((order) => OrderActions.activateVariantSuccess({ order })),
          catchError(() => of(OrderActions.activateVariantFailure({ error: 'Не вдалося відновити варіант.' }))),
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

  loadCities$ = createEffect(() =>
    this.actions$.pipe(
      ofType(OrderActions.loadCities),
      switchMap(({ query }) =>
        this.orders.novaPoshtaCities(query).pipe(
          map((cities) => OrderActions.loadCitiesSuccess({ cities })),
          catchError(() => of(OrderActions.loadCitiesFailure({ error: 'Не вдалося знайти міста.' }))),
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
      switchMap(({ orderId, delivery }) =>
        this.orders.checkout(orderId, delivery).pipe(
          catchError(() => {
            throw { step: 'checkout' as const };
          }),
          switchMap(() =>
            this.orders.pay(orderId).pipe(
              catchError(() => {
                throw { step: 'pay' as const };
              }),
            ),
          ),
          // Hard navigation, not a router link — the destination is Monobank's hosted payment
          // page (or, in the no-merchant-token local-dev fallback, straight back to /status).
          tap(({ pageUrl }) => {
            window.location.href = pageUrl;
          }),
          map(() => OrderActions.checkoutAndPaySuccess()),
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

  setPrintQuantity$ = createEffect(() =>
    this.actions$.pipe(
      ofType(OrderActions.setPrintQuantity),
      mergeMap(({ orderId, quantity }) =>
        this.orders.setPrintQuantity(orderId, quantity).pipe(
          map(() => OrderActions.setPrintQuantitySuccess({ orderId, quantity })),
          catchError(() => of(OrderActions.setPrintQuantityFailure({ error: 'Не вдалося змінити кількість.' }))),
        ),
      ),
    ),
  );

  checkoutAndPayBatch$ = createEffect(() =>
    this.actions$.pipe(
      ofType(OrderActions.checkoutAndPayBatch),
      switchMap(({ orderIds, delivery }) =>
        this.orders.checkoutBatch(orderIds, delivery).pipe(
          catchError(() => {
            throw { step: 'checkout' as const };
          }),
          switchMap(() =>
            this.orders.payBatch(orderIds).pipe(
              catchError(() => {
                throw { step: 'pay' as const };
              }),
            ),
          ),
          tap(({ pageUrl }) => {
            window.location.href = pageUrl;
          }),
          map(() => OrderActions.checkoutAndPayBatchSuccess()),
          catchError((err: { step?: 'checkout' | 'pay' }) =>
            of(
              OrderActions.checkoutAndPayBatchFailure({
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

  archiveOrder$ = createEffect(() =>
    this.actions$.pipe(
      ofType(OrderActions.archiveOrder),
      mergeMap(({ orderId }) =>
        this.orders.archiveOrder(orderId).pipe(
          map(() => OrderActions.archiveOrderSuccess({ orderId })),
          catchError(() => of(OrderActions.archiveOrderFailure({ error: 'Не вдалося архівувати замовлення.' }))),
        ),
      ),
    ),
  );

  unarchiveOrder$ = createEffect(() =>
    this.actions$.pipe(
      ofType(OrderActions.unarchiveOrder),
      mergeMap(({ orderId }) =>
        this.orders.unarchiveOrder(orderId).pipe(
          map(() => OrderActions.unarchiveOrderSuccess({ orderId })),
          catchError(() => of(OrderActions.unarchiveOrderFailure({ error: 'Не вдалося відновити замовлення.' }))),
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
