import { createActionGroup, emptyProps, props } from '@ngrx/store';
import { AdminOrderSummaryDto, AdminUserDto, ImageGenerationProvider, OrderDto, PagedResult } from '../../models';

export const AdminActions = createActionGroup({
  source: 'Admin',
  events: {
    'Load Orders': props<{ page: number; pageSize: number; status?: string }>(),
    'Load Orders Success': props<{ result: PagedResult<AdminOrderSummaryDto> }>(),
    'Load Orders Failure': props<{ error: string }>(),

    'Load Users': props<{ page: number; pageSize: number }>(),
    'Load Users Success': props<{ result: PagedResult<AdminUserDto> }>(),
    'Load Users Failure': props<{ error: string }>(),

    'Load Order Detail': props<{ orderId: string }>(),
    'Load Order Detail Success': props<{ order: OrderDto }>(),
    'Load Order Detail Failure': props<{ error: string }>(),

    'Replace Photo': props<{ orderId: string; photo: File }>(),
    'Replace Photo Success': props<{ order: OrderDto }>(),
    'Replace Photo Failure': props<{ error: string }>(),

    'Regenerate Sheet': props<{ orderId: string; sheetId: string }>(),
    'Regenerate Sheet Success': props<{ order: OrderDto }>(),
    'Regenerate Sheet Failure': props<{ error: string }>(),

    'Load Ai Provider': emptyProps(),
    'Load Ai Provider Success': props<{ provider: ImageGenerationProvider }>(),
    'Load Ai Provider Failure': props<{ error: string }>(),

    'Set Ai Provider': props<{ provider: ImageGenerationProvider }>(),
    'Set Ai Provider Success': props<{ provider: ImageGenerationProvider }>(),
    'Set Ai Provider Failure': props<{ error: string }>(),

    'Clear Admin Error': emptyProps(),
  },
});
