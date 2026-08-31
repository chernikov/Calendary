import { createActionGroup, emptyProps, props } from '@ngrx/store';
import { NovaPoshtaWarehouseDto, OrderDto, StyleCategoryDto } from '../../models';

export const OrderActions = createActionGroup({
  source: 'Order',
  events: {
    'Load Order': props<{ orderId: string }>(),
    'Load Order Success': props<{ order: OrderDto }>(),
    'Load Order Failure': props<{ error: string }>(),

    'Start Order Polling': props<{ orderId: string; intervalMs: number }>(),
    'Stop Order Polling': emptyProps(),

    'Load Style Categories': emptyProps(),
    'Load Style Categories Success': props<{ categories: StyleCategoryDto[] }>(),
    'Load Style Categories Failure': props<{ error: string }>(),

    'Upload Photo': props<{ orderId: string; photoDataUrl: string }>(),
    'Upload Photo Success': props<{ order: OrderDto }>(),
    'Upload Photo Failure': props<{ error: string }>(),

    'Select Style': props<{ orderId: string; styleCategoryId: string }>(),
    'Select Style Success': props<{ order: OrderDto }>(),
    'Select Style Failure': props<{ error: string }>(),

    'Add Personal Date': props<{ orderId: string; day: number; month: number; label: string }>(),
    'Add Personal Date Success': props<{ order: OrderDto }>(),
    'Add Personal Date Failure': props<{ error: string }>(),

    'Remove Personal Date': props<{ orderId: string; dateId: string }>(),
    'Remove Personal Date Success': props<{ order: OrderDto }>(),
    'Remove Personal Date Failure': props<{ error: string }>(),

    'Start Generation': props<{ orderId: string }>(),
    'Start Generation Success': props<{ order: OrderDto }>(),
    'Start Generation Failure': props<{ error: string }>(),

    'Regenerate Sheet': props<{ orderId: string; sheetId: string }>(),
    'Regenerate Sheet Success': props<{ order: OrderDto }>(),
    'Regenerate Sheet Failure': props<{ error: string }>(),

    'Confirm Cover': props<{ orderId: string; sheetId: string }>(),
    'Confirm Cover Success': props<{ order: OrderDto }>(),
    'Confirm Cover Failure': props<{ error: string }>(),

    'Load Warehouses': props<{ city: string }>(),
    'Load Warehouses Success': props<{ warehouses: NovaPoshtaWarehouseDto[] }>(),
    'Load Warehouses Failure': props<{ error: string }>(),
    'Clear Warehouses': emptyProps(),

    'Checkout And Pay': props<{
      orderId: string;
      delivery: { recipientName: string; phone: string; city: string; warehouseNumber: string; warehouseAddress: string };
      method: string;
    }>(),
    'Checkout And Pay Success': props<{ order: OrderDto }>(),
    'Checkout And Pay Failure': props<{ error: string }>(),

    'Cancel Order': props<{ orderId: string }>(),
    'Cancel Order Success': props<{ order: OrderDto }>(),
    'Cancel Order Failure': props<{ error: string }>(),

    'Download Pdf': props<{ orderId: string }>(),
    'Download Pdf Success': emptyProps(),
    'Download Pdf Failure': props<{ error: string }>(),

    'Clear Order Error': emptyProps(),
  },
});
