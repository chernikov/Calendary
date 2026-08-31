import { createReducer, on } from '@ngrx/store';
import { OrderActions } from './order.actions';
import { initialOrderState } from './order.state';

export const orderReducer = createReducer(
  initialOrderState,

  on(OrderActions.loadOrderSuccess, (state, { order }) => ({ ...state, order, error: null })),
  on(OrderActions.loadOrderFailure, (state, { error }) => ({ ...state, error })),

  on(OrderActions.loadMyOrders, (state) => ({ ...state, busy: true, error: null })),
  on(OrderActions.loadMyOrdersSuccess, (state, { orders }) => ({ ...state, myOrders: orders, busy: false })),
  on(OrderActions.loadMyOrdersFailure, (state, { error }) => ({ ...state, busy: false, error })),

  on(OrderActions.loadPromptLibrarySuccess, (state, { library }) => ({ ...state, promptLibrary: library })),
  on(OrderActions.loadPromptLibraryFailure, (state, { error }) => ({ ...state, error })),

  on(
    OrderActions.uploadPhoto,
    OrderActions.startGeneration,
    OrderActions.savePlanAndGenerate,
    OrderActions.regenerateSheet,
    OrderActions.confirmCover,
    OrderActions.checkoutAndPay,
    OrderActions.cancelOrder,
    (state) => ({ ...state, busy: true, error: null }),
  ),

  on(
    OrderActions.uploadPhotoSuccess,
    OrderActions.addPersonalDateSuccess,
    OrderActions.removePersonalDateSuccess,
    OrderActions.startGenerationSuccess,
    OrderActions.generateSheetSuccess,
    OrderActions.regenerateSheetSuccess,
    OrderActions.confirmCoverSuccess,
    OrderActions.checkoutAndPaySuccess,
    OrderActions.cancelOrderSuccess,
    (state, { order }) => ({ ...state, order, busy: false, error: null }),
  ),

  on(
    OrderActions.uploadPhotoFailure,
    OrderActions.addPersonalDateFailure,
    OrderActions.removePersonalDateFailure,
    OrderActions.startGenerationFailure,
    OrderActions.savePlanAndGenerateFailure,
    OrderActions.generateSheetFailure,
    OrderActions.regenerateSheetFailure,
    OrderActions.confirmCoverFailure,
    OrderActions.checkoutAndPayFailure,
    OrderActions.cancelOrderFailure,
    (state, { error }) => ({ ...state, busy: false, error }),
  ),

  on(OrderActions.loadWarehousesSuccess, (state, { warehouses }) => ({ ...state, warehouses })),
  on(OrderActions.loadWarehousesFailure, (state) => ({ ...state, warehouses: [] })),
  on(OrderActions.clearWarehouses, (state) => ({ ...state, warehouses: [] })),

  on(OrderActions.downloadPdf, (state) => ({ ...state, downloadingPdf: true, error: null })),
  on(OrderActions.downloadPdfSuccess, (state) => ({ ...state, downloadingPdf: false })),
  on(OrderActions.downloadPdfFailure, (state, { error }) => ({ ...state, downloadingPdf: false, error })),

  on(OrderActions.clearOrderError, (state) => ({ ...state, error: null })),
);
