import { AdminOrderSummaryDto, AdminUserDto, ImageGenerationProvider, OrderDto, PagedResult } from '../../models';

export interface AdminState {
  orders: PagedResult<AdminOrderSummaryDto> | null;
  users: PagedResult<AdminUserDto> | null;
  selectedOrder: OrderDto | null;
  aiProvider: ImageGenerationProvider | null;
  busy: boolean;
  error: string | null;
}

export const initialAdminState: AdminState = {
  orders: null,
  users: null,
  selectedOrder: null,
  aiProvider: null,
  busy: false,
  error: null,
};

export const ADMIN_FEATURE_KEY = 'admin';
