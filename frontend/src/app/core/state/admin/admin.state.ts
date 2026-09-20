import {
  AdminOrderSummaryDto,
  AdminUserDto,
  BackupStatusDto,
  ConfigStatusDto,
  HolidayDto,
  ImageGenerationProvider,
  ImageStyleDto,
  OrderDto,
  PagedResult,
  PromoCodeDto,
  PromptThemeDto,
} from '../../models';

export interface AdminState {
  orders: PagedResult<AdminOrderSummaryDto> | null;
  users: PagedResult<AdminUserDto> | null;
  selectedOrder: OrderDto | null;
  basePrice: number | null;
  aiProvider: ImageGenerationProvider | null;
  configStatus: ConfigStatusDto | null;
  backupStatus: BackupStatusDto | null;
  promptThemes: PromptThemeDto[];
  imageStyles: ImageStyleDto[];
  holidays: HolidayDto[];
  promoCodes: PromoCodeDto[];
  busy: boolean;
  error: string | null;
}

export const initialAdminState: AdminState = {
  orders: null,
  users: null,
  selectedOrder: null,
  basePrice: null,
  aiProvider: null,
  configStatus: null,
  backupStatus: null,
  promptThemes: [],
  imageStyles: [],
  holidays: [],
  promoCodes: [],
  busy: false,
  error: null,
};

export const ADMIN_FEATURE_KEY = 'admin';
