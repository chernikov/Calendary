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
}
