import { createActionGroup, emptyProps, props } from '@ngrx/store';
import { HolidayDto, NovaPoshtaWarehouseDto, OrderDto, OrderSummaryDto, PromptLibraryDto, SheetPlanItem } from '../../models';

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

    'Load Holidays': props<{ year: number }>(),
    'Load Holidays Success': props<{ holidays: HolidayDto[] }>(),
    'Load Holidays Failure': props<{ error: string }>(),

    // The order isn't created until a photo is actually attached — see #348.
    'Create Order With Photo': props<{ photo: File }>(),
    'Create Order With Photo Success': props<{ order: OrderDto }>(),
    'Create Order With Photo Failure': props<{ error: string }>(),

    // Photos upload one at a time (see #347) — added to an already-created order.
    'Add Order Photo': props<{ orderId: string; photo: File }>(),
    'Add Order Photo Success': props<{ order: OrderDto }>(),
    'Add Order Photo Failure': props<{ error: string }>(),

    'Remove Order Photo': props<{ orderId: string; photoId: string }>(),
    'Remove Order Photo Success': props<{ order: OrderDto }>(),
    'Remove Order Photo Failure': props<{ error: string }>(),

    'Save Plan And Generate': props<{ orderId: string; items: SheetPlanItem[] }>(),
    'Save Plan And Generate Failure': props<{ error: string }>(),

    'Add Personal Date': props<{ orderId: string; day: number; month: number; label: string }>(),
    'Add Personal Date Success': props<{ order: OrderDto }>(),
    'Add Personal Date Failure': props<{ error: string }>(),

    'Remove Personal Date': props<{ orderId: string; dateId: string }>(),
    'Remove Personal Date Success': props<{ order: OrderDto }>(),
    'Remove Personal Date Failure': props<{ error: string }>(),

    'Save Holiday Settings': props<{ orderId: string; countries: string[]; weekStart: string }>(),
    'Save Holiday Settings Success': props<{ order: OrderDto }>(),
    'Save Holiday Settings Failure': props<{ error: string }>(),

    'Start Generation': props<{ orderId: string }>(),
    'Start Generation Success': props<{ order: OrderDto }>(),
    'Start Generation Failure': props<{ error: string }>(),

    // The single generation trigger everywhere (see #351) — a sheet's first variant is free, any
    // variant after that spends a regeneration; the server decides which.
    'Generate Sheet': props<{ orderId: string; index: number; promptId: string; imageStyleId: string; photoId?: string }>(),
    'Generate Sheet Success': props<{ order: OrderDto }>(),
    'Generate Sheet Failure': props<{ error: string }>(),

    // Restores a previously generated variant as active — free, no generation involved.
    'Activate Variant': props<{ orderId: string; sheetId: string; variantId: string }>(),
    'Activate Variant Success': props<{ order: OrderDto }>(),
    'Activate Variant Failure': props<{ error: string }>(),

    'Confirm Cover': props<{ orderId: string; sheetId: string }>(),
    'Confirm Cover Success': props<{ order: OrderDto }>(),
    'Confirm Cover Failure': props<{ error: string }>(),

    'Load Cities': props<{ query: string }>(),
    'Load Cities Success': props<{ cities: string[] }>(),
    'Load Cities Failure': props<{ error: string }>(),
    'Clear Cities': emptyProps(),

    'Load Warehouses': props<{ city: string }>(),
    'Load Warehouses Success': props<{ warehouses: NovaPoshtaWarehouseDto[] }>(),
    'Load Warehouses Failure': props<{ error: string }>(),
    'Clear Warehouses': emptyProps(),

    'Checkout And Pay': props<{
      orderId: string;
      delivery: { recipientName: string; phone: string; city: string; warehouseNumber: string; warehouseAddress: string };
    }>(),
    'Checkout And Pay Success': emptyProps(),
    'Checkout And Pay Failure': props<{ error: string }>(),

    'Cancel Order': props<{ orderId: string }>(),
    'Cancel Order Success': props<{ order: OrderDto }>(),
    'Cancel Order Failure': props<{ error: string }>(),

    'Archive Order': props<{ orderId: string }>(),
    'Archive Order Success': props<{ orderId: string }>(),
    'Archive Order Failure': props<{ error: string }>(),

    'Unarchive Order': props<{ orderId: string }>(),
    'Unarchive Order Success': props<{ orderId: string }>(),
    'Unarchive Order Failure': props<{ error: string }>(),

    'Download Pdf': props<{ orderId: string }>(),
    'Download Pdf Success': emptyProps(),
    'Download Pdf Failure': props<{ error: string }>(),

    'Clear Order Error': emptyProps(),
  },
});
