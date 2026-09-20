export type UserRole = 'Customer' | 'Admin';

export interface UserDto {
  id: string;
  displayName: string | null;
  email: string | null;
  emailConfirmed: boolean;
  role: UserRole;
}

export interface PromptDto {
  id: string;
  promptThemeId: string;
  name: string;
  text: string;
  description: string;
  previewImageUrl: string | null;
  sortOrder: number;
}

export interface PromptThemeDto {
  id: string;
  name: string;
  description: string;
  sortOrder: number;
  prompts: PromptDto[];
}

export interface ImageStyleDto {
  id: string;
  name: string;
  text: string;
  description: string;
  previewImageUrl: string | null;
  sortOrder: number;
}

export interface PromptLibraryDto {
  themes: PromptThemeDto[];
  styles: ImageStyleDto[];
}

export interface SheetPlanItem {
  index: number;
  promptId: string;
  imageStyleId: string;
  photoId?: string;
}

export interface PersonalDateDto {
  id: string;
  day: number;
  month: number;
  label: string;
}

export type SheetKind = 'Cover' | 'Month';
export type SheetStatus = 'Pending' | 'Generating' | 'Ready' | 'Failed';

export interface SheetVariantDto {
  id: string;
  imageUrl: string;
  createdAtUtc: string;
  costUsd: number | null;
}

export interface SheetDto {
  id: string;
  kind: SheetKind;
  index: number;
  status: SheetStatus;
  isSelected: boolean;
  imageUrl: string | null;
  promptId: string | null;
  promptName: string | null;
  imageStyleId: string | null;
  imageStyleName: string | null;
  photoId: string | null;
  activeVariantId: string | null;
  variants: SheetVariantDto[];
}

export interface PaymentDto {
  method: string;
  status: string;
  amount: number;
  paidAtUtc: string | null;
}

export interface DeliveryDto {
  recipientName: string;
  phone: string;
  city: string;
  warehouseNumber: string;
  warehouseAddress: string;
  trackingNumber: string | null;
}

export type OrderStatus =
  | 'Created'
  | 'PhotoUploaded'
  | 'DetailsSubmitted'
  | 'Generating'
  | 'CoverReady'
  | 'CoverConfirmed'
  | 'ReviewReady'
  | 'AwaitingPayment'
  | 'Paid'
  | 'Printing'
  | 'Shipped'
  | 'Delivered'
  | 'Cancelled'
  | 'GenerationFailed';

export interface OrderPhotoDto {
  id: string;
  url: string;
  thumbUrl: string;
}

export interface OrderDto {
  id: string;
  status: OrderStatus;
  photos: OrderPhotoDto[];
  price: number;
  regenerationsRemaining: number;
  totalGenerationCostUsd: number;
  createdAtUtc: string;
  expiresAtUtc: string;
  isExpired: boolean;
  personalDates: PersonalDateDto[];
  sheets: SheetDto[];
  payment: PaymentDto | null;
  delivery: DeliveryDto | null;
  holidayCountries: string[];
  weekStart: string;
}

export interface OrderSummaryDto {
  id: string;
  status: OrderStatus;
  price: number;
  createdAtUtc: string;
  statusUpdatedAtUtc: string;
  styleName: string | null;
  coverImageUrl: string | null;
  isArchived: boolean;
}

export interface NovaPoshtaWarehouseDto {
  number: string;
  address: string;
  closesAt: string;
  isPostomat: boolean;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
}

export interface AdminOrderSummaryDto {
  id: string;
  status: OrderStatus;
  userId: string;
  userEmail: string | null;
  userDisplayName: string | null;
  price: number;
  totalGenerationCostUsd: number;
  createdAtUtc: string;
  statusUpdatedAtUtc: string;
}

export interface AdminUserDto {
  id: string;
  email: string | null;
  displayName: string | null;
  role: UserRole;
  authProvider: 'Password' | 'Google';
  emailConfirmed: boolean;
  createdAtUtc: string;
  orderCount: number;
}

export type ImageGenerationProvider = 'Mock' | 'OpenAI' | 'Gemini';

export interface ConfigStatusDto {
  openAiConfigured: boolean;
  geminiConfigured: boolean;
  googleConfigured: boolean;
  resendConfigured: boolean;
  monobankConfigured: boolean;
}

export interface BackupSnapshotDto {
  timeUtc: string;
  tags: string[];
}

export interface BackupStatusDto {
  configured: boolean;
  snapshots: BackupSnapshotDto[];
}

export interface SavePromptThemePayload {
  id?: string;
  name: string;
  description: string;
  sortOrder: number;
}

export interface SavePromptPayload {
  id?: string;
  promptThemeId: string;
  name: string;
  text: string;
  description: string;
  previewImageUrl: string | null;
  sortOrder: number;
}

export interface SaveImageStylePayload {
  id?: string;
  name: string;
  text: string;
  description: string;
  previewImageUrl: string | null;
  sortOrder: number;
}

export interface HolidayDto {
  id: string;
  country: string;
  year: number;
  day: number;
  month: number;
  name: string;
  shortName: string;
}

export interface SaveHolidayPayload {
  id?: string;
  country: string;
  year: number;
  day: number;
  month: number;
  name: string;
  shortName: string;
}
