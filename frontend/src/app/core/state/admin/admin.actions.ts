import { createActionGroup, emptyProps, props } from '@ngrx/store';
import {
  AdminOrderSummaryDto,
  AdminUserDto,
  BackupStatusDto,
  ConfigStatusDto,
  HolidayDto,
  ImageGenerationProvider,
  ImageStyleDto,
  OrderDto,
  OrderStatusHistoryEntryDto,
  PagedResult,
  PromoCodeDto,
  PromptThemeDto,
  SaveHolidayPayload,
  SaveImageStylePayload,
  SavePromoCodePayload,
  SavePromptPayload,
  SavePromptThemePayload,
} from '../../models';

export const AdminActions = createActionGroup({
  source: 'Admin',
  events: {
    'Load Orders': props<{ page: number; pageSize: number; status?: string; search?: string }>(),
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

    'Advance Fulfillment': props<{ orderId: string }>(),
    'Advance Fulfillment Success': props<{ order: OrderDto }>(),
    'Advance Fulfillment Failure': props<{ error: string }>(),

    'Load Order Status History': props<{ orderId: string }>(),
    'Load Order Status History Success': props<{ history: OrderStatusHistoryEntryDto[] }>(),
    'Load Order Status History Failure': props<{ error: string }>(),

    'Load Product Settings': emptyProps(),
    'Load Product Settings Success': props<{ basePrice: number }>(),
    'Load Product Settings Failure': props<{ error: string }>(),

    'Set Product Settings': props<{ basePrice: number }>(),
    'Set Product Settings Success': props<{ basePrice: number }>(),
    'Set Product Settings Failure': props<{ error: string }>(),

    'Load Ai Provider': emptyProps(),
    'Load Ai Provider Success': props<{ provider: ImageGenerationProvider }>(),
    'Load Ai Provider Failure': props<{ error: string }>(),

    'Set Ai Provider': props<{ provider: ImageGenerationProvider }>(),
    'Set Ai Provider Success': props<{ provider: ImageGenerationProvider }>(),
    'Set Ai Provider Failure': props<{ error: string }>(),

    'Load Config Status': emptyProps(),
    'Load Config Status Success': props<{ status: ConfigStatusDto }>(),
    'Load Config Status Failure': props<{ error: string }>(),

    'Load Backup Status': emptyProps(),
    'Load Backup Status Success': props<{ status: BackupStatusDto }>(),
    'Load Backup Status Failure': props<{ error: string }>(),

    'Load Prompt Themes': emptyProps(),
    'Load Prompt Themes Success': props<{ themes: PromptThemeDto[] }>(),
    'Load Prompt Themes Failure': props<{ error: string }>(),

    'Save Prompt Theme': props<{ theme: SavePromptThemePayload }>(),
    'Delete Prompt Theme': props<{ themeId: string }>(),
    'Save Prompt': props<{ prompt: SavePromptPayload }>(),
    'Delete Prompt': props<{ promptId: string }>(),
    'Generate Prompt Preview': props<{ promptId: string }>(),
    'Prompt Library Mutation Failure': props<{ error: string }>(),

    'Load Image Styles': emptyProps(),
    'Load Image Styles Success': props<{ styles: ImageStyleDto[] }>(),
    'Load Image Styles Failure': props<{ error: string }>(),

    'Save Image Style': props<{ style: SaveImageStylePayload }>(),
    'Delete Image Style': props<{ styleId: string }>(),
    'Generate Image Style Preview': props<{ styleId: string }>(),

    'Load Holidays': emptyProps(),
    'Load Holidays Success': props<{ holidays: HolidayDto[] }>(),
    'Load Holidays Failure': props<{ error: string }>(),

    'Save Holiday': props<{ holiday: SaveHolidayPayload }>(),
    'Delete Holiday': props<{ holidayId: string }>(),
    'Holiday Mutation Failure': props<{ error: string }>(),

    'Load Promo Codes': emptyProps(),
    'Load Promo Codes Success': props<{ promoCodes: PromoCodeDto[] }>(),
    'Load Promo Codes Failure': props<{ error: string }>(),

    'Save Promo Code': props<{ promoCode: SavePromoCodePayload }>(),
    'Delete Promo Code': props<{ promoCodeId: string }>(),
    'Promo Code Mutation Failure': props<{ error: string }>(),

    'Clear Admin Error': emptyProps(),
  },
});
