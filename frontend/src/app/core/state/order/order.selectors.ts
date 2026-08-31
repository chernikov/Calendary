import { createFeatureSelector, createSelector } from '@ngrx/store';
import { SheetDto } from '../../models';
import { ORDER_FEATURE_KEY, OrderState } from './order.state';

export const selectOrderState = createFeatureSelector<OrderState>(ORDER_FEATURE_KEY);

export const selectOrder = createSelector(selectOrderState, (state) => state.order);
export const selectMyOrders = createSelector(selectOrderState, (state) => state.myOrders);
export const selectPromptLibrary = createSelector(selectOrderState, (state) => state.promptLibrary);
export const selectWarehouses = createSelector(selectOrderState, (state) => state.warehouses);
export const selectOrderBusy = createSelector(selectOrderState, (state) => state.busy);
export const selectDownloadingPdf = createSelector(selectOrderState, (state) => state.downloadingPdf);
export const selectOrderError = createSelector(selectOrderState, (state) => state.error);

export const selectCoverSheet = createSelector(selectOrder, (order): SheetDto | undefined =>
  order?.sheets.find((s) => s.kind === 'Cover'),
);

export const selectSheetForMonth = (month: number) =>
  createSelector(selectOrder, (order): SheetDto | undefined =>
    order?.sheets.find((s) => s.kind === 'Month' && s.index === month),
  );

export const selectPersonalDatesForMonth = (month: number) =>
  createSelector(selectOrder, (order) => (order?.personalDates ?? []).filter((d) => d.month === month));
