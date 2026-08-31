import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import {
  AdminOrderSummaryDto,
  AdminUserDto,
  ImageGenerationProvider,
  ImageStyleDto,
  OrderDto,
  PagedResult,
  PromptDto,
  PromptThemeDto,
  SaveImageStylePayload,
  SavePromptPayload,
  SavePromptThemePayload,
} from './models';

const BASE = `${environment.apiBaseUrl}/api/admin`;

@Injectable({ providedIn: 'root' })
export class AdminService {
  constructor(private readonly http: HttpClient) {}

  listOrders(page: number, pageSize: number, status?: string): Observable<PagedResult<AdminOrderSummaryDto>> {
    let params = new HttpParams().set('page', page).set('pageSize', pageSize);
    if (status) {
      params = params.set('status', status);
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

  listUsers(page: number, pageSize: number): Observable<PagedResult<AdminUserDto>> {
    const params = new HttpParams().set('page', page).set('pageSize', pageSize);
    return this.http.get<PagedResult<AdminUserDto>>(`${BASE}/users`, { params });
  }

  getAiProvider(): Observable<{ provider: ImageGenerationProvider }> {
    return this.http.get<{ provider: ImageGenerationProvider }>(`${BASE}/settings/ai-provider`);
  }

  setAiProvider(provider: ImageGenerationProvider): Observable<{ provider: ImageGenerationProvider }> {
    return this.http.put<{ provider: ImageGenerationProvider }>(`${BASE}/settings/ai-provider`, { provider });
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
}
