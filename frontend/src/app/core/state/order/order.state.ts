import { NovaPoshtaWarehouseDto, OrderDto, OrderSummaryDto, PromptLibraryDto } from '../../models';

export interface OrderState {
  order: OrderDto | null;
  myOrders: OrderSummaryDto[];
  promptLibrary: PromptLibraryDto | null;
  warehouses: NovaPoshtaWarehouseDto[];
  busy: boolean;
  downloadingPdf: boolean;
  error: string | null;
}

export const initialOrderState: OrderState = {
  order: null,
  myOrders: [],
  promptLibrary: null,
  warehouses: [],
  busy: false,
  downloadingPdf: false,
  error: null,
};

export const ORDER_FEATURE_KEY = 'order';
