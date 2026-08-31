import { createFeatureSelector, createSelector } from '@ngrx/store';
import { ADMIN_FEATURE_KEY, AdminState } from './admin.state';

export const selectAdminState = createFeatureSelector<AdminState>(ADMIN_FEATURE_KEY);

export const selectAdminOrders = createSelector(selectAdminState, (state) => state.orders);
export const selectAdminUsers = createSelector(selectAdminState, (state) => state.users);
export const selectAdminSelectedOrder = createSelector(selectAdminState, (state) => state.selectedOrder);
export const selectAdminAiProvider = createSelector(selectAdminState, (state) => state.aiProvider);
export const selectAdminPromptThemes = createSelector(selectAdminState, (state) => state.promptThemes);
export const selectAdminImageStyles = createSelector(selectAdminState, (state) => state.imageStyles);
export const selectAdminBusy = createSelector(selectAdminState, (state) => state.busy);
export const selectAdminError = createSelector(selectAdminState, (state) => state.error);
