import { createReducer, on } from '@ngrx/store';
import { AdminActions } from './admin.actions';
import { initialAdminState } from './admin.state';

export const adminReducer = createReducer(
  initialAdminState,

  on(
    AdminActions.loadOrders,
    AdminActions.loadUsers,
    AdminActions.replacePhoto,
    AdminActions.regenerateSheet,
    AdminActions.setAiProvider,
    AdminActions.savePromptTheme,
    AdminActions.deletePromptTheme,
    AdminActions.savePrompt,
    AdminActions.deletePrompt,
    AdminActions.saveImageStyle,
    AdminActions.deleteImageStyle,
    (state) => ({ ...state, busy: true, error: null }),
  ),

  // Real order payloads can be tens of MB (base64-encoded generated images) and take many
  // seconds to load — clear the previously viewed order so switching orders doesn't flash stale
  // data for the whole load, and the template's "order() is null" check can drive a spinner.
  on(AdminActions.loadOrderDetail, (state) => ({ ...state, selectedOrder: null, busy: true, error: null })),

  on(AdminActions.loadOrdersSuccess, (state, { result }) => ({ ...state, orders: result, busy: false, error: null })),
  on(AdminActions.loadUsersSuccess, (state, { result }) => ({ ...state, users: result, busy: false, error: null })),

  on(
    AdminActions.loadOrderDetailSuccess,
    AdminActions.replacePhotoSuccess,
    AdminActions.regenerateSheetSuccess,
    (state, { order }) => ({ ...state, selectedOrder: order, busy: false, error: null }),
  ),

  on(AdminActions.loadAiProviderSuccess, AdminActions.setAiProviderSuccess, (state, { provider }) => ({
    ...state,
    aiProvider: provider,
    busy: false,
    error: null,
  })),

  on(AdminActions.loadPromptThemesSuccess, (state, { themes }) => ({
    ...state,
    promptThemes: themes,
    busy: false,
    error: null,
  })),

  on(AdminActions.loadImageStylesSuccess, (state, { styles }) => ({
    ...state,
    imageStyles: styles,
    busy: false,
    error: null,
  })),

  on(
    AdminActions.loadOrdersFailure,
    AdminActions.loadUsersFailure,
    AdminActions.loadOrderDetailFailure,
    AdminActions.replacePhotoFailure,
    AdminActions.regenerateSheetFailure,
    AdminActions.loadAiProviderFailure,
    AdminActions.setAiProviderFailure,
    AdminActions.loadPromptThemesFailure,
    AdminActions.loadImageStylesFailure,
    AdminActions.promptLibraryMutationFailure,
    (state, { error }) => ({ ...state, busy: false, error }),
  ),

  on(AdminActions.clearAdminError, (state) => ({ ...state, error: null })),
);
