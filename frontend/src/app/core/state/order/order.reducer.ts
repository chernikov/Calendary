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

  on(OrderActions.loadHolidaysSuccess, (state, { holidays }) => ({ ...state, holidays })),
  on(OrderActions.loadHolidaysFailure, (state, { error }) => ({ ...state, error })),

  on(
    OrderActions.createOrderWithPhoto,
    OrderActions.addOrderPhoto,
    OrderActions.removeOrderPhoto,
    OrderActions.startGeneration,
    OrderActions.savePlanAndGenerate,
    OrderActions.activateVariant,
    OrderActions.confirmCover,
    OrderActions.checkoutAndPay,
    OrderActions.cancelOrder,
    (state) => ({ ...state, busy: true, error: null }),
  ),

  on(
    OrderActions.createOrderWithPhotoSuccess,
    OrderActions.addOrderPhotoSuccess,
    OrderActions.removeOrderPhotoSuccess,
    OrderActions.addPersonalDateSuccess,
    OrderActions.removePersonalDateSuccess,
    OrderActions.saveHolidaySettingsSuccess,
    OrderActions.startGenerationSuccess,
    OrderActions.generateSheetSuccess,
    OrderActions.activateVariantSuccess,
    OrderActions.confirmCoverSuccess,
    OrderActions.cancelOrderSuccess,
    (state, { order }) => ({ ...state, order, busy: false, error: null }),
  ),

  // No updated order comes back — the browser is about to hard-navigate to Monobank's hosted
  // page (or straight to /status in the local-dev fallback), so there's nothing left to reflect.
  on(OrderActions.checkoutAndPaySuccess, (state) => ({ ...state, busy: false, error: null })),

  on(
    OrderActions.createOrderWithPhotoFailure,
    OrderActions.addOrderPhotoFailure,
    OrderActions.removeOrderPhotoFailure,
    OrderActions.addPersonalDateFailure,
    OrderActions.removePersonalDateFailure,
    OrderActions.saveHolidaySettingsFailure,
    OrderActions.startGenerationFailure,
    OrderActions.savePlanAndGenerateFailure,
    OrderActions.generateSheetFailure,
    OrderActions.activateVariantFailure,
    OrderActions.confirmCoverFailure,
    OrderActions.checkoutAndPayFailure,
    OrderActions.cancelOrderFailure,
    (state, { error }) => ({ ...state, busy: false, error }),
  ),

  on(OrderActions.archiveOrderSuccess, (state, { orderId }) => ({
    ...state,
    myOrders: state.myOrders.map((o) => (o.id === orderId ? { ...o, isArchived: true } : o)),
  })),
  on(OrderActions.unarchiveOrderSuccess, (state, { orderId }) => ({
    ...state,
    myOrders: state.myOrders.map((o) => (o.id === orderId ? { ...o, isArchived: false } : o)),
  })),
  on(OrderActions.archiveOrderFailure, OrderActions.unarchiveOrderFailure, (state, { error }) => ({ ...state, error })),

  on(OrderActions.loadCitiesSuccess, (state, { cities }) => ({ ...state, cities })),
  on(OrderActions.loadCitiesFailure, (state) => ({ ...state, cities: [] })),
  on(OrderActions.clearCities, (state) => ({ ...state, cities: [] })),

  on(OrderActions.loadWarehousesSuccess, (state, { warehouses }) => ({ ...state, warehouses })),
  on(OrderActions.loadWarehousesFailure, (state) => ({ ...state, warehouses: [] })),
  on(OrderActions.clearWarehouses, (state) => ({ ...state, warehouses: [] })),

  on(OrderActions.downloadPdf, (state) => ({ ...state, downloadingPdf: true, error: null })),
  on(OrderActions.downloadPdfSuccess, (state) => ({ ...state, downloadingPdf: false })),
  on(OrderActions.downloadPdfFailure, (state, { error }) => ({ ...state, downloadingPdf: false, error })),

  on(OrderActions.clearOrderError, (state) => ({ ...state, error: null })),
);
