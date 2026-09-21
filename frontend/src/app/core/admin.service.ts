import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
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
  ProductSettingsDto,
  PromoCodeDto,
  PromptDto,
  PromptThemeDto,
  RealIntegrationsOnStagingDto,
  SaveHolidayPayload,
  SaveImageStylePayload,
  SavePromoCodePayload,
  SavePromptPayload,
  SavePromptThemePayload,
} from './models';

const BASE = `${environment.apiBaseUrl}/api/admin`;

@Injectable({ providedIn: 'root' })
export class AdminService {
  constructor(private readonly http: HttpClient) {}

  listOrders(page: number, pageSize: number, status?: string, search?: string): Observable<PagedResult<AdminOrderSummaryDto>> {
    let params = new HttpParams().set('page', page).set('pageSize', pageSize);
    if (status) {
      params = params.set('status', status);
    }
    if (search) {
      params = params.set('search', search);
    }
    return this.http.get<PagedResult<AdminOrderSummaryDto>>(`${BASE}/orders`, { params });
  }

  getOrder(orderId: string): Observable<OrderDto> {
    return this.http.get<OrderDto>(`${BASE}/orders/${orderId}`);
  }

  replacePhoto(orderId: string, photo: File): Observable<OrderDto> {
    const form = new FormData();
    form.append('photo', photo, photo.name);
    return this.http.post<OrderDto>(`${BASE}/orders/${orderId}/photo`, form);
  }

  regenerateSheet(orderId: string, sheetId: string): Observable<OrderDto> {
    return this.http.post<OrderDto>(`${BASE}/orders/${orderId}/sheets/${sheetId}/regenerate`, {});
  }

  advanceFulfillment(orderId: string): Observable<OrderDto> {
    return this.http.post<OrderDto>(`${BASE}/orders/${orderId}/advance-fulfillment`, {});
  }

  getOrderStatusHistory(orderId: string): Observable<OrderStatusHistoryEntryDto[]> {
    return this.http.get<OrderStatusHistoryEntryDto[]>(`${BASE}/orders/${orderId}/status-history`);
  }

  listUsers(page: number, pageSize: number): Observable<PagedResult<AdminUserDto>> {
    const params = new HttpParams().set('page', page).set('pageSize', pageSize);
    return this.http.get<PagedResult<AdminUserDto>>(`${BASE}/users`, { params });
  }

  getProduct(): Observable<ProductSettingsDto> {
    return this.http.get<ProductSettingsDto>(`${BASE}/product`);
  }

  setProduct(basePrice: number): Observable<ProductSettingsDto> {
    return this.http.put<ProductSettingsDto>(`${BASE}/product`, { basePrice });
  }

  getAiProvider(): Observable<{ provider: ImageGenerationProvider }> {
    return this.http.get<{ provider: ImageGenerationProvider }>(`${BASE}/settings/ai-provider`);
  }

  setAiProvider(provider: ImageGenerationProvider): Observable<{ provider: ImageGenerationProvider }> {
    return this.http.put<{ provider: ImageGenerationProvider }>(`${BASE}/settings/ai-provider`, { provider });
  }

  getConfigStatus(): Observable<ConfigStatusDto> {
    return this.http.get<ConfigStatusDto>(`${BASE}/settings/config-status`);
  }

  getRealIntegrationsOnStaging(): Observable<RealIntegrationsOnStagingDto> {
    return this.http.get<RealIntegrationsOnStagingDto>(`${BASE}/settings/real-integrations-on-staging`);
  }

  setRealIntegrationsOnStaging(enabled: boolean): Observable<RealIntegrationsOnStagingDto> {
    return this.http.put<RealIntegrationsOnStagingDto>(`${BASE}/settings/real-integrations-on-staging`, { enabled });
  }

  getBackupStatus(): Observable<BackupStatusDto> {
    return this.http.get<BackupStatusDto>(`${BASE}/settings/backup-status`);
  }

  listPromptThemes(): Observable<PromptThemeDto[]> {
    return this.http.get<PromptThemeDto[]>(`${BASE}/prompt-themes`);
  }

  savePromptTheme(theme: SavePromptThemePayload): Observable<PromptThemeDto> {
    return theme.id
      ? this.http.put<PromptThemeDto>(`${BASE}/prompt-themes/${theme.id}`, theme)
      : this.http.post<PromptThemeDto>(`${BASE}/prompt-themes`, theme);
  }

  deletePromptTheme(themeId: string): Observable<void> {
    return this.http.delete<void>(`${BASE}/prompt-themes/${themeId}`);
  }

  savePrompt(prompt: SavePromptPayload): Observable<PromptDto> {
    return prompt.id
      ? this.http.put<PromptDto>(`${BASE}/prompts/${prompt.id}`, prompt)
      : this.http.post<PromptDto>(`${BASE}/prompts`, prompt);
  }

  deletePrompt(promptId: string): Observable<void> {
    return this.http.delete<void>(`${BASE}/prompts/${promptId}`);
  }

  generatePromptPreview(promptId: string): Observable<PromptDto> {
    return this.http.post<PromptDto>(`${BASE}/prompts/${promptId}/generate-preview`, {});
  }

  listImageStyles(): Observable<ImageStyleDto[]> {
    return this.http.get<ImageStyleDto[]>(`${BASE}/image-styles`);
  }

  saveImageStyle(style: SaveImageStylePayload): Observable<ImageStyleDto> {
    return style.id
      ? this.http.put<ImageStyleDto>(`${BASE}/image-styles/${style.id}`, style)
      : this.http.post<ImageStyleDto>(`${BASE}/image-styles`, style);
  }

  deleteImageStyle(styleId: string): Observable<void> {
    return this.http.delete<void>(`${BASE}/image-styles/${styleId}`);
  }

  generateImageStylePreview(styleId: string): Observable<ImageStyleDto> {
    return this.http.post<ImageStyleDto>(`${BASE}/image-styles/${styleId}/generate-preview`, {});
  }

  listHolidays(): Observable<HolidayDto[]> {
    return this.http.get<HolidayDto[]>(`${BASE}/holidays`);
  }

  saveHoliday(holiday: SaveHolidayPayload): Observable<HolidayDto> {
    return holiday.id
      ? this.http.put<HolidayDto>(`${BASE}/holidays/${holiday.id}`, holiday)
      : this.http.post<HolidayDto>(`${BASE}/holidays`, holiday);
  }

  deleteHoliday(holidayId: string): Observable<void> {
    return this.http.delete<void>(`${BASE}/holidays/${holidayId}`);
  }

  listPromoCodes(): Observable<PromoCodeDto[]> {
    return this.http.get<PromoCodeDto[]>(`${BASE}/promo-codes`);
  }

  savePromoCode(promoCode: SavePromoCodePayload): Observable<PromoCodeDto> {
    return promoCode.id
      ? this.http.put<PromoCodeDto>(`${BASE}/promo-codes/${promoCode.id}`, promoCode)
      : this.http.post<PromoCodeDto>(`${BASE}/promo-codes`, promoCode);
  }

  deletePromoCode(promoCodeId: string): Observable<void> {
    return this.http.delete<void>(`${BASE}/promo-codes/${promoCodeId}`);
  }
}
