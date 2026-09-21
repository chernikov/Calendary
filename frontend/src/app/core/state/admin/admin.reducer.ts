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
    AdminActions.advanceFulfillment,
    AdminActions.setAiProvider,
    AdminActions.setRealIntegrationsOnStaging,
    AdminActions.setProductSettings,
    AdminActions.savePromptTheme,
    AdminActions.deletePromptTheme,
    AdminActions.savePrompt,
    AdminActions.deletePrompt,
    AdminActions.generatePromptPreview,
    AdminActions.saveImageStyle,
    AdminActions.deleteImageStyle,
    AdminActions.generateImageStylePreview,
    AdminActions.saveHoliday,
    AdminActions.deleteHoliday,
    AdminActions.savePromoCode,
    AdminActions.deletePromoCode,
    AdminActions.generateExperimentalImage,
    (state) => ({ ...state, busy: true, error: null }),
  ),

  // Real order payloads can be tens of MB (base64-encoded generated images) and take many
  // seconds to load — clear the previously viewed order so switching orders doesn't flash stale
  // data for the whole load, and the template's "order() is null" check can drive a spinner.
  on(AdminActions.loadOrderDetail, (state) => ({
    ...state,
    selectedOrder: null,
    selectedOrderStatusHistory: [],
    busy: true,
    error: null,
  })),

  on(AdminActions.loadOrdersSuccess, (state, { result }) => ({ ...state, orders: result, busy: false, error: null })),
  on(AdminActions.loadUsersSuccess, (state, { result }) => ({ ...state, users: result, busy: false, error: null })),

  on(
    AdminActions.loadOrderDetailSuccess,
    AdminActions.replacePhotoSuccess,
    AdminActions.regenerateSheetSuccess,
    AdminActions.advanceFulfillmentSuccess,
    (state, { order }) => ({ ...state, selectedOrder: order, busy: false, error: null }),
  ),

  on(AdminActions.loadOrderStatusHistorySuccess, (state, { history }) => ({
    ...state,
    selectedOrderStatusHistory: history,
  })),

  on(AdminActions.loadAiProviderSuccess, AdminActions.setAiProviderSuccess, (state, { provider }) => ({
    ...state,
    aiProvider: provider,
    busy: false,
    error: null,
  })),

  on(AdminActions.loadProductSettingsSuccess, AdminActions.setProductSettingsSuccess, (state, { basePrice }) => ({
    ...state,
    basePrice,
    busy: false,
    error: null,
  })),

  on(AdminActions.loadConfigStatusSuccess, (state, { status }) => ({ ...state, configStatus: status, error: null })),
  on(AdminActions.loadBackupStatusSuccess, (state, { status }) => ({ ...state, backupStatus: status, error: null })),

  on(
    AdminActions.loadRealIntegrationsOnStagingSuccess,
    AdminActions.setRealIntegrationsOnStagingSuccess,
    (state, { enabled }) => ({ ...state, realIntegrationsOnStaging: enabled, busy: false, error: null }),
  ),

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

  on(AdminActions.loadHolidaysSuccess, (state, { holidays }) => ({
    ...state,
    holidays,
    busy: false,
    error: null,
  })),

  on(AdminActions.loadPromoCodesSuccess, (state, { promoCodes }) => ({
    ...state,
    promoCodes,
    busy: false,
    error: null,
  })),

  on(
    AdminActions.loadOrdersFailure,
    AdminActions.loadUsersFailure,
    AdminActions.loadOrderDetailFailure,
    AdminActions.replacePhotoFailure,
    AdminActions.regenerateSheetFailure,
    AdminActions.advanceFulfillmentFailure,
    AdminActions.loadOrderStatusHistoryFailure,
    AdminActions.loadAiProviderFailure,
    AdminActions.setAiProviderFailure,
    AdminActions.loadProductSettingsFailure,
    AdminActions.setProductSettingsFailure,
    AdminActions.loadConfigStatusFailure,
    AdminActions.loadBackupStatusFailure,
    AdminActions.loadRealIntegrationsOnStagingFailure,
    AdminActions.setRealIntegrationsOnStagingFailure,
    AdminActions.loadPromptThemesFailure,
    AdminActions.loadImageStylesFailure,
    AdminActions.promptLibraryMutationFailure,
    AdminActions.loadHolidaysFailure,
    AdminActions.holidayMutationFailure,
    AdminActions.loadPromoCodesFailure,
    AdminActions.promoCodeMutationFailure,
    AdminActions.generateExperimentalImageFailure,
    (state, { error }) => ({ ...state, busy: false, error }),
  ),

  on(AdminActions.generateExperimentalImageSuccess, (state, { result }) => ({
    ...state,
    experimentalGenerationResult: result,
    busy: false,
    error: null,
  })),

  on(AdminActions.clearAdminError, (state) => ({ ...state, error: null })),
);
