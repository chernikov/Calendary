export interface UserDto {
  id: string;
  displayName: string | null;
  email: string | null;
  phone: string | null;
}

export interface StyleCategoryDto {
  id: string;
  code: string;
  name: string;
  description: string;
  sortOrder: number;
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
  styleCategory: StyleCategoryDto | null;
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

export interface NovaPoshtaWarehouseDto {
  number: string;
  address: string;
  closesAt: string;
}
