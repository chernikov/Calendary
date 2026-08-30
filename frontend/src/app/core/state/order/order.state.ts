import { NovaPoshtaWarehouseDto, OrderDto, StyleCategoryDto } from '../../models';

export interface OrderState {
  order: OrderDto | null;
  styleCategories: StyleCategoryDto[];
  warehouses: NovaPoshtaWarehouseDto[];
  busy: boolean;
  error: string | null;
}

export const initialOrderState: OrderState = {
  order: null,
  styleCategories: [],
  warehouses: [],
  busy: false,
  error: null,
};

export const ORDER_FEATURE_KEY = 'order';
