import { createActionGroup, emptyProps, props } from '@ngrx/store';
import { NovaPoshtaWarehouseDto, OrderDto, OrderSummaryDto, PromptLibraryDto, SheetPlanItem } from '../../models';

export const OrderActions = createActionGroup({
  source: 'Order',
  events: {
    'Load Order': props<{ orderId: string }>(),
    'Load Order Success': props<{ order: OrderDto }>(),
    'Load Order Failure': props<{ error: string }>(),

    'Start Order Polling': props<{ orderId: string; intervalMs: number }>(),
    'Stop Order Polling': emptyProps(),

    'Load My Orders': emptyProps(),
    'Load My Orders Success': props<{ orders: OrderSummaryDto[] }>(),
    'Load My Orders Failure': props<{ error: string }>(),

    'Load Prompt Library': emptyProps(),
    'Load Prompt Library Success': props<{ library: PromptLibraryDto }>(),
    'Load Prompt Library Failure': props<{ error: string }>(),

    'Upload Photo': props<{ orderId: string; photo: File }>(),
    'Upload Photo Success': props<{ order: OrderDto }>(),
    'Upload Photo Failure': props<{ error: string }>(),

    'Save Plan And Generate': props<{ orderId: string; items: SheetPlanItem[] }>(),
    'Save Plan And Generate Failure': props<{ error: string }>(),

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
