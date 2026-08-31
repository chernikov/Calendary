import {
  AdminOrderSummaryDto,
  AdminUserDto,
  ImageGenerationProvider,
  ImageStyleDto,
  OrderDto,
  PagedResult,
  PromptThemeDto,
} from '../../models';

export interface AdminState {
  orders: PagedResult<AdminOrderSummaryDto> | null;
  users: PagedResult<AdminUserDto> | null;
  selectedOrder: OrderDto | null;
  aiProvider: ImageGenerationProvider | null;
  promptThemes: PromptThemeDto[];
  imageStyles: ImageStyleDto[];
  busy: boolean;
  error: string | null;
}

export const initialAdminState: AdminState = {
  orders: null,
  users: null,
  selectedOrder: null,
  aiProvider: null,
  promptThemes: [],
  imageStyles: [],
  busy: false,
  error: null,
};

export const ADMIN_FEATURE_KEY = 'admin';
