import { createActionGroup, emptyProps, props } from '@ngrx/store';
import {
  AdminOrderSummaryDto,
  AdminUserDto,
  BackupStatusDto,
  ConfigStatusDto,
  ImageGenerationProvider,
  ImageStyleDto,
  OrderDto,
  PagedResult,
  PromptThemeDto,
  SaveImageStylePayload,
  SavePromptPayload,
  SavePromptThemePayload,
} from '../../models';

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
    'Prompt Library Mutation Failure': props<{ error: string }>(),

    'Load Image Styles': emptyProps(),
    'Load Image Styles Success': props<{ styles: ImageStyleDto[] }>(),
    'Load Image Styles Failure': props<{ error: string }>(),

    'Save Image Style': props<{ style: SaveImageStylePayload }>(),
    'Delete Image Style': props<{ styleId: string }>(),

    'Clear Admin Error': emptyProps(),
  },
});
