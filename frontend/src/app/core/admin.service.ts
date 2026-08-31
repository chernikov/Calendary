import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { AdminOrderSummaryDto, AdminUserDto, ImageGenerationProvider, OrderDto, PagedResult } from './models';

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

  replacePhoto(orderId: string, photoDataUrl: string): Observable<OrderDto> {
    return this.http.post<OrderDto>(`${BASE}/orders/${orderId}/photo`, { photoDataUrl });
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
}
