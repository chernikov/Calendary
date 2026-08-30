import { createReducer, on } from '@ngrx/store';
import { OrderActions } from './order.actions';
import { initialOrderState } from './order.state';

export const orderReducer = createReducer(
  initialOrderState,

  on(OrderActions.loadOrderSuccess, (state, { order }) => ({ ...state, order, error: null })),
  on(OrderActions.loadOrderFailure, (state, { error }) => ({ ...state, error })),

  on(OrderActions.loadStyleCategoriesSuccess, (state, { categories }) => ({ ...state, styleCategories: categories })),
  on(OrderActions.loadStyleCategoriesFailure, (state, { error }) => ({ ...state, error })),

  on(
    OrderActions.uploadPhoto,
    OrderActions.startGeneration,
    OrderActions.regenerateSheet,
    OrderActions.confirmCover,
    OrderActions.checkoutAndPay,
    OrderActions.cancelOrder,
    (state) => ({ ...state, busy: true, error: null }),
  ),

  on(
    OrderActions.uploadPhotoSuccess,
    OrderActions.selectStyleSuccess,
    OrderActions.addPersonalDateSuccess,
    OrderActions.removePersonalDateSuccess,
    OrderActions.startGenerationSuccess,
    OrderActions.regenerateSheetSuccess,
    OrderActions.confirmCoverSuccess,
    OrderActions.checkoutAndPaySuccess,
    OrderActions.cancelOrderSuccess,
    (state, { order }) => ({ ...state, order, busy: false, error: null }),
  ),

  on(
    OrderActions.uploadPhotoFailure,
    OrderActions.selectStyleFailure,
    OrderActions.addPersonalDateFailure,
    OrderActions.removePersonalDateFailure,
    OrderActions.startGenerationFailure,
    OrderActions.regenerateSheetFailure,
    OrderActions.confirmCoverFailure,
    OrderActions.checkoutAndPayFailure,
    OrderActions.cancelOrderFailure,
    (state, { error }) => ({ ...state, busy: false, error }),
  ),

  on(OrderActions.loadWarehousesSuccess, (state, { warehouses }) => ({ ...state, warehouses })),
  on(OrderActions.loadWarehousesFailure, (state) => ({ ...state, warehouses: [] })),
  on(OrderActions.clearWarehouses, (state) => ({ ...state, warehouses: [] })),

  on(OrderActions.clearOrderError, (state) => ({ ...state, error: null })),
);
