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
}

export interface PersonalDateDto {
  id: string;
  day: number;
  month: number;
  label: string;
}

export type SheetKind = 'Cover' | 'Month';
export type SheetStatus = 'Pending' | 'Generating' | 'Ready' | 'Failed';

export interface SheetDto {
  id: string;
  kind: SheetKind;
  index: number;
  status: SheetStatus;
  isSelected: boolean;
  imageUrl: string | null;
  variantCount: number;
  promptId: string | null;
  promptName: string | null;
  imageStyleId: string | null;
  imageStyleName: string | null;
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

export interface OrderDto {
  id: string;
  status: OrderStatus;
  photoUrl: string | null;
  price: number;
  regenerationsRemaining: number;
  createdAtUtc: string;
  expiresAtUtc: string;
  isExpired: boolean;
  personalDates: PersonalDateDto[];
  sheets: SheetDto[];
  payment: PaymentDto | null;
  delivery: DeliveryDto | null;
}

export interface OrderSummaryDto {
  id: string;
  status: OrderStatus;
  price: number;
  createdAtUtc: string;
  statusUpdatedAtUtc: string;
  styleName: string | null;
  coverImageUrl: string | null;
}

export interface NovaPoshtaWarehouseDto {
  number: string;
  address: string;
  closesAt: string;
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
