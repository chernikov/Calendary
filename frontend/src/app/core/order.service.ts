import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import {
  NovaPoshtaWarehouseDto,
  OrderDto,
  OrderSummaryDto,
  StyleCategoryDto,
} from './models';

const BASE = `${environment.apiBaseUrl}/api`;

@Injectable({ providedIn: 'root' })
export class OrderService {
  constructor(private readonly http: HttpClient) {}

  styleCategories(): Observable<StyleCategoryDto[]> {
    return this.http.get<StyleCategoryDto[]>(`${BASE}/style-categories`);
  }

  createOrder(): Observable<OrderDto> {
    return this.http.post<OrderDto>(`${BASE}/orders`, {});
  }

  getOrder(orderId: string): Observable<OrderDto> {
    return this.http.get<OrderDto>(`${BASE}/orders/${orderId}`);
  }

  listOrders(): Observable<OrderSummaryDto[]> {
    return this.http.get<OrderSummaryDto[]>(`${BASE}/orders`);
  }

  uploadPhoto(orderId: string, photo: File): Observable<OrderDto> {
    const form = new FormData();
    form.append('photo', photo, photo.name);
    return this.http.post<OrderDto>(`${BASE}/orders/${orderId}/photo`, form);
  }

  selectStyle(orderId: string, styleCategoryId: string): Observable<OrderDto> {
    return this.http.post<OrderDto>(`${BASE}/orders/${orderId}/style`, { styleCategoryId });
  }

  addDate(orderId: string, day: number, month: number, label: string): Observable<OrderDto> {
    return this.http.post<OrderDto>(`${BASE}/orders/${orderId}/dates`, { day, month, label });
  }

  removeDate(orderId: string, dateId: string): Observable<OrderDto> {
    return this.http.delete<OrderDto>(`${BASE}/orders/${orderId}/dates/${dateId}`);
  }

  startGeneration(orderId: string): Observable<OrderDto> {
    return this.http.post<OrderDto>(`${BASE}/orders/${orderId}/generate`, {});
  }

  regenerateSheet(orderId: string, sheetId: string): Observable<OrderDto> {
    return this.http.post<OrderDto>(`${BASE}/orders/${orderId}/sheets/${sheetId}/regenerate`, {});
  }

  simulateFailure(orderId: string, sheetId: string): Observable<OrderDto> {
    return this.http.post<OrderDto>(`${BASE}/orders/${orderId}/sheets/${sheetId}/simulate-failure`, {});
  }

  confirmCover(orderId: string, sheetId: string): Observable<OrderDto> {
    return this.http.post<OrderDto>(`${BASE}/orders/${orderId}/cover/confirm`, { sheetId });
  }

  checkout(
    orderId: string,
    delivery: { recipientName: string; phone: string; city: string; warehouseNumber: string; warehouseAddress: string },
  ): Observable<OrderDto> {
    return this.http.post<OrderDto>(`${BASE}/orders/${orderId}/checkout`, delivery);
  }

  pay(orderId: string, method: string): Observable<OrderDto> {
    return this.http.post<OrderDto>(`${BASE}/orders/${orderId}/pay`, { method });
  }

  cancel(orderId: string): Observable<OrderDto> {
    return this.http.post<OrderDto>(`${BASE}/orders/${orderId}/cancel`, {});
  }

  downloadPdf(orderId: string): Observable<Blob> {
    return this.http.get(`${BASE}/orders/${orderId}/pdf`, { responseType: 'blob' });
  }

  novaPoshtaCities(query: string): Observable<string[]> {
    return this.http.get<string[]>(`${BASE}/nova-poshta/cities`, { params: { query } });
  }

  novaPoshtaWarehouses(city: string): Observable<NovaPoshtaWarehouseDto[]> {
    return this.http.get<NovaPoshtaWarehouseDto[]>(`${BASE}/nova-poshta/warehouses`, { params: { city } });
  }
}
