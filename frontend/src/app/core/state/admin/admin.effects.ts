import { HttpErrorResponse } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { catchError, map, of, switchMap } from 'rxjs';
import { AdminService } from '../../admin.service';
import { photoUploadErrorMessage } from '../../photo-upload-error';
import { AdminActions } from './admin.actions';

@Injectable()
export class AdminEffects {
  private readonly actions$ = inject(Actions);
  private readonly admin = inject(AdminService);

  loadOrders$ = createEffect(() =>
    this.actions$.pipe(
      ofType(AdminActions.loadOrders),
      switchMap(({ page, pageSize, status, search }) =>
        this.admin.listOrders(page, pageSize, status, search).pipe(
          map((result) => AdminActions.loadOrdersSuccess({ result })),
          catchError(() => of(AdminActions.loadOrdersFailure({ error: 'Не вдалося завантажити замовлення.' }))),
        ),
      ),
    ),
  );

  loadUsers$ = createEffect(() =>
    this.actions$.pipe(
      ofType(AdminActions.loadUsers),
      switchMap(({ page, pageSize }) =>
        this.admin.listUsers(page, pageSize).pipe(
          map((result) => AdminActions.loadUsersSuccess({ result })),
          catchError(() => of(AdminActions.loadUsersFailure({ error: 'Не вдалося завантажити користувачів.' }))),
        ),
      ),
    ),
  );

  loadOrderDetail$ = createEffect(() =>
    this.actions$.pipe(
      ofType(AdminActions.loadOrderDetail),
      switchMap(({ orderId }) =>
        this.admin.getOrder(orderId).pipe(
          map((order) => AdminActions.loadOrderDetailSuccess({ order })),
          catchError(() => of(AdminActions.loadOrderDetailFailure({ error: 'Не вдалося завантажити замовлення.' }))),
        ),
      ),
    ),
  );

  loadHistoryAlongsideOrderDetail$ = createEffect(() =>
    this.actions$.pipe(
      ofType(AdminActions.loadOrderDetail),
      map(({ orderId }) => AdminActions.loadOrderStatusHistory({ orderId })),
    ),
  );

  replacePhoto$ = createEffect(() =>
    this.actions$.pipe(
      ofType(AdminActions.replacePhoto),
      switchMap(({ orderId, photo }) =>
        this.admin.replacePhoto(orderId, photo).pipe(
          map((order) => AdminActions.replacePhotoSuccess({ order })),
          catchError((err) => of(AdminActions.replacePhotoFailure({ error: photoUploadErrorMessage(err) }))),
        ),
      ),
    ),
  );

  regenerateSheet$ = createEffect(() =>
    this.actions$.pipe(
      ofType(AdminActions.regenerateSheet),
      switchMap(({ orderId, sheetId }) =>
        this.admin.regenerateSheet(orderId, sheetId).pipe(
          map((order) => AdminActions.regenerateSheetSuccess({ order })),
          catchError(() => of(AdminActions.regenerateSheetFailure({ error: 'Перегенерації вичерпано.' }))),
        ),
      ),
    ),
  );

  advanceFulfillment$ = createEffect(() =>
    this.actions$.pipe(
      ofType(AdminActions.advanceFulfillment),
      switchMap(({ orderId }) =>
        this.admin.advanceFulfillment(orderId).pipe(
          map((order) => AdminActions.advanceFulfillmentSuccess({ order })),
          catchError((err: HttpErrorResponse) =>
            of(AdminActions.advanceFulfillmentFailure({
              error: typeof err.error === 'string' ? err.error : 'Не вдалося перейти на наступний етап.',
            })),
          ),
        ),
      ),
    ),
  );

  // Status history changed too, alongside the order itself — refetch it so the timeline reflects
  // the step that was just applied without a page reload.
  refreshHistoryAfterAdvance$ = createEffect(() =>
    this.actions$.pipe(
      ofType(AdminActions.advanceFulfillmentSuccess),
      map(({ order }) => AdminActions.loadOrderStatusHistory({ orderId: order.id })),
    ),
  );

  loadOrderStatusHistory$ = createEffect(() =>
    this.actions$.pipe(
      ofType(AdminActions.loadOrderStatusHistory),
      switchMap(({ orderId }) =>
        this.admin.getOrderStatusHistory(orderId).pipe(
          map((history) => AdminActions.loadOrderStatusHistorySuccess({ history })),
          catchError(() => of(AdminActions.loadOrderStatusHistoryFailure({ error: 'Не вдалося завантажити історію статусів.' }))),
        ),
      ),
    ),
  );

  loadProductSettings$ = createEffect(() =>
    this.actions$.pipe(
      ofType(AdminActions.loadProductSettings),
      switchMap(() =>
        this.admin.getProduct().pipe(
          map(({ basePrice }) => AdminActions.loadProductSettingsSuccess({ basePrice })),
          catchError(() => of(AdminActions.loadProductSettingsFailure({ error: 'Не вдалося завантажити ціну товару.' }))),
        ),
      ),
    ),
  );

  setProductSettings$ = createEffect(() =>
    this.actions$.pipe(
      ofType(AdminActions.setProductSettings),
      switchMap(({ basePrice }) =>
        this.admin.setProduct(basePrice).pipe(
          map(({ basePrice: updated }) => AdminActions.setProductSettingsSuccess({ basePrice: updated })),
          catchError(() => of(AdminActions.setProductSettingsFailure({ error: 'Не вдалося змінити ціну товару.' }))),
        ),
      ),
    ),
  );

  loadAiProvider$ = createEffect(() =>
    this.actions$.pipe(
      ofType(AdminActions.loadAiProvider),
      switchMap(() =>
        this.admin.getAiProvider().pipe(
          map(({ provider }) => AdminActions.loadAiProviderSuccess({ provider })),
          catchError(() => of(AdminActions.loadAiProviderFailure({ error: 'Не вдалося завантажити налаштування.' }))),
        ),
      ),
    ),
  );

  setAiProvider$ = createEffect(() =>
    this.actions$.pipe(
      ofType(AdminActions.setAiProvider),
      switchMap(({ provider }) =>
        this.admin.setAiProvider(provider).pipe(
          map(({ provider: updated }) => AdminActions.setAiProviderSuccess({ provider: updated })),
          catchError(() => of(AdminActions.setAiProviderFailure({ error: 'Не вдалося змінити провайдера.' }))),
        ),
      ),
    ),
  );

  loadConfigStatus$ = createEffect(() =>
    this.actions$.pipe(
      ofType(AdminActions.loadConfigStatus),
      switchMap(() =>
        this.admin.getConfigStatus().pipe(
          map((status) => AdminActions.loadConfigStatusSuccess({ status })),
          catchError(() => of(AdminActions.loadConfigStatusFailure({ error: 'Не вдалося перевірити ключі.' }))),
        ),
      ),
    ),
  );

  loadBackupStatus$ = createEffect(() =>
    this.actions$.pipe(
      ofType(AdminActions.loadBackupStatus),
      switchMap(() =>
        this.admin.getBackupStatus().pipe(
          map((status) => AdminActions.loadBackupStatusSuccess({ status })),
          catchError(() => of(AdminActions.loadBackupStatusFailure({ error: 'Не вдалося перевірити бекапи.' }))),
        ),
      ),
    ),
  );

  loadPromptThemes$ = createEffect(() =>
    this.actions$.pipe(
      ofType(AdminActions.loadPromptThemes),
      switchMap(() =>
        this.admin.listPromptThemes().pipe(
          map((themes) => AdminActions.loadPromptThemesSuccess({ themes })),
          catchError(() => of(AdminActions.loadPromptThemesFailure({ error: 'Не вдалося завантажити теми.' }))),
        ),
      ),
    ),
  );

  // Every prompt-library mutation reloads the theme list — payloads are tiny and it keeps the
  // store consistent without per-mutation success reducers.
  savePromptTheme$ = createEffect(() =>
    this.actions$.pipe(
      ofType(AdminActions.savePromptTheme),
      switchMap(({ theme }) =>
        this.admin.savePromptTheme(theme).pipe(
          map(() => AdminActions.loadPromptThemes()),
          catchError(() => of(AdminActions.promptLibraryMutationFailure({ error: 'Не вдалося зберегти тему.' }))),
        ),
      ),
    ),
  );

  deletePromptTheme$ = createEffect(() =>
    this.actions$.pipe(
      ofType(AdminActions.deletePromptTheme),
      switchMap(({ themeId }) =>
        this.admin.deletePromptTheme(themeId).pipe(
          map(() => AdminActions.loadPromptThemes()),
          catchError((err) =>
            of(
              AdminActions.promptLibraryMutationFailure({
                error:
                  err?.status === 409
                    ? 'Тему не можна видалити: її промпти вже використані в замовленнях.'
                    : 'Не вдалося видалити тему.',
              }),
            ),
          ),
        ),
      ),
    ),
  );

  savePrompt$ = createEffect(() =>
    this.actions$.pipe(
      ofType(AdminActions.savePrompt),
      switchMap(({ prompt }) =>
        this.admin.savePrompt(prompt).pipe(
          map(() => AdminActions.loadPromptThemes()),
          catchError(() => of(AdminActions.promptLibraryMutationFailure({ error: 'Не вдалося зберегти промпт.' }))),
        ),
      ),
    ),
  );

  deletePrompt$ = createEffect(() =>
    this.actions$.pipe(
      ofType(AdminActions.deletePrompt),
      switchMap(({ promptId }) =>
        this.admin.deletePrompt(promptId).pipe(
          map(() => AdminActions.loadPromptThemes()),
          catchError((err) =>
            of(
              AdminActions.promptLibraryMutationFailure({
                error:
                  err?.status === 409
                    ? 'Промпт не можна видалити: він уже використаний в замовленнях.'
                    : 'Не вдалося видалити промпт.',
              }),
            ),
          ),
        ),
      ),
    ),
  );

  loadImageStyles$ = createEffect(() =>
    this.actions$.pipe(
      ofType(AdminActions.loadImageStyles),
      switchMap(() =>
        this.admin.listImageStyles().pipe(
          map((styles) => AdminActions.loadImageStylesSuccess({ styles })),
          catchError(() => of(AdminActions.loadImageStylesFailure({ error: 'Не вдалося завантажити стилі.' }))),
        ),
      ),
    ),
  );

  saveImageStyle$ = createEffect(() =>
    this.actions$.pipe(
      ofType(AdminActions.saveImageStyle),
      switchMap(({ style }) =>
        this.admin.saveImageStyle(style).pipe(
          map(() => AdminActions.loadImageStyles()),
          catchError(() => of(AdminActions.promptLibraryMutationFailure({ error: 'Не вдалося зберегти стиль.' }))),
        ),
      ),
    ),
  );

  generatePromptPreview$ = createEffect(() =>
    this.actions$.pipe(
      ofType(AdminActions.generatePromptPreview),
      switchMap(({ promptId }) =>
        this.admin.generatePromptPreview(promptId).pipe(
          map(() => AdminActions.loadPromptThemes()),
          catchError((err: HttpErrorResponse) =>
            of(AdminActions.promptLibraryMutationFailure({
              error: typeof err.error === 'string' ? err.error : 'Не вдалося згенерувати приклад.',
            })),
          ),
        ),
      ),
    ),
  );

  generateImageStylePreview$ = createEffect(() =>
    this.actions$.pipe(
      ofType(AdminActions.generateImageStylePreview),
      switchMap(({ styleId }) =>
        this.admin.generateImageStylePreview(styleId).pipe(
          map(() => AdminActions.loadImageStyles()),
          catchError((err: HttpErrorResponse) =>
            of(AdminActions.promptLibraryMutationFailure({
              error: typeof err.error === 'string' ? err.error : 'Не вдалося згенерувати приклад.',
            })),
          ),
        ),
      ),
    ),
  );

  deleteImageStyle$ = createEffect(() =>
    this.actions$.pipe(
      ofType(AdminActions.deleteImageStyle),
      switchMap(({ styleId }) =>
        this.admin.deleteImageStyle(styleId).pipe(
          map(() => AdminActions.loadImageStyles()),
          catchError((err) =>
            of(
              AdminActions.promptLibraryMutationFailure({
                error:
                  err?.status === 409
                    ? 'Стиль не можна видалити: він уже використаний в замовленнях.'
                    : 'Не вдалося видалити стиль.',
              }),
            ),
          ),
        ),
      ),
    ),
  );

  loadHolidays$ = createEffect(() =>
    this.actions$.pipe(
      ofType(AdminActions.loadHolidays),
      switchMap(() =>
        this.admin.listHolidays().pipe(
          map((holidays) => AdminActions.loadHolidaysSuccess({ holidays })),
          catchError(() => of(AdminActions.loadHolidaysFailure({ error: 'Не вдалося завантажити свята.' }))),
        ),
      ),
    ),
  );

  saveHoliday$ = createEffect(() =>
    this.actions$.pipe(
      ofType(AdminActions.saveHoliday),
      switchMap(({ holiday }) =>
        this.admin.saveHoliday(holiday).pipe(
          map(() => AdminActions.loadHolidays()),
          catchError(() => of(AdminActions.holidayMutationFailure({ error: 'Не вдалося зберегти свято.' }))),
        ),
      ),
    ),
  );

  deleteHoliday$ = createEffect(() =>
    this.actions$.pipe(
      ofType(AdminActions.deleteHoliday),
      switchMap(({ holidayId }) =>
        this.admin.deleteHoliday(holidayId).pipe(
          map(() => AdminActions.loadHolidays()),
          catchError(() => of(AdminActions.holidayMutationFailure({ error: 'Не вдалося видалити свято.' }))),
        ),
      ),
    ),
  );

  loadPromoCodes$ = createEffect(() =>
    this.actions$.pipe(
      ofType(AdminActions.loadPromoCodes),
      switchMap(() =>
        this.admin.listPromoCodes().pipe(
          map((promoCodes) => AdminActions.loadPromoCodesSuccess({ promoCodes })),
          catchError(() => of(AdminActions.loadPromoCodesFailure({ error: 'Не вдалося завантажити промокоди.' }))),
        ),
      ),
    ),
  );

  savePromoCode$ = createEffect(() =>
    this.actions$.pipe(
      ofType(AdminActions.savePromoCode),
      switchMap(({ promoCode }) =>
        this.admin.savePromoCode(promoCode).pipe(
          map(() => AdminActions.loadPromoCodes()),
          catchError((err: HttpErrorResponse) =>
            of(AdminActions.promoCodeMutationFailure({
              error: typeof err.error === 'string' ? err.error : 'Не вдалося зберегти промокод.',
            })),
          ),
        ),
      ),
    ),
  );

  deletePromoCode$ = createEffect(() =>
    this.actions$.pipe(
      ofType(AdminActions.deletePromoCode),
      switchMap(({ promoCodeId }) =>
        this.admin.deletePromoCode(promoCodeId).pipe(
          map(() => AdminActions.loadPromoCodes()),
          catchError(() => of(AdminActions.promoCodeMutationFailure({ error: 'Не вдалося видалити промокод.' }))),
        ),
      ),
    ),
  );
}
