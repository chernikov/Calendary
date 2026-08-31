import { NovaPoshtaWarehouseDto, OrderDto, OrderSummaryDto, StyleCategoryDto } from '../../models';

export interface OrderState {
  order: OrderDto | null;
  myOrders: OrderSummaryDto[];
  styleCategories: StyleCategoryDto[];
  warehouses: NovaPoshtaWarehouseDto[];
  busy: boolean;
  downloadingPdf: boolean;
  error: string | null;
}

export const initialOrderState: OrderState = {
  order: null,
  myOrders: [],
  styleCategories: [],
  warehouses: [],
  busy: false,
  downloadingPdf: false,
  error: null,
};

export const ORDER_FEATURE_KEY = 'order';
