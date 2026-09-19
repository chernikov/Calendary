import {
  AdminOrderSummaryDto,
  AdminUserDto,
  ConfigStatusDto,
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
  configStatus: ConfigStatusDto | null;
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
  configStatus: null,
  promptThemes: [],
  imageStyles: [],
  busy: false,
  error: null,
};

export const ADMIN_FEATURE_KEY = 'admin';
