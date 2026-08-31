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
      switchMap(({ page, pageSize, status }) =>
        this.admin.listOrders(page, pageSize, status).pipe(
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
}
