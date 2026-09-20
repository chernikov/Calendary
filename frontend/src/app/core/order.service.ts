import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import {
  HolidayDto,
  NovaPoshtaWarehouseDto,
  OrderDto,
  OrderSummaryDto,
  PromptLibraryDto,
  SheetPlanItem,
} from './models';

const BASE = `${environment.apiBaseUrl}/api`;

@Injectable({ providedIn: 'root' })
export class OrderService {
  constructor(private readonly http: HttpClient) {}

  promptLibrary(): Observable<PromptLibraryDto> {
    return this.http.get<PromptLibraryDto>(`${BASE}/prompt-library`);
  }

  // Fetches every country's holidays for the year in one call — the style-dates step filters
  // client-side by the customer's current country selection, so toggling a checkbox updates the
  // calendar preview instantly (see #366).
  listHolidays(year: number): Observable<HolidayDto[]> {
    return this.http.get<HolidayDto[]>(`${BASE}/holidays`, { params: { year } });
  }

  // The order isn't created until a photo is actually attached — see #348.
  createOrder(photo: File): Observable<OrderDto> {
    const form = new FormData();
    form.append('photo', photo, photo.name);
    return this.http.post<OrderDto>(`${BASE}/orders`, form);
  }

  // Photos are uploaded one at a time (see #347) — the first creates the order (above), any
  // further photo is added to it here.
  addPhoto(orderId: string, photo: File): Observable<OrderDto> {
    const form = new FormData();
    form.append('photo', photo, photo.name);
    return this.http.post<OrderDto>(`${BASE}/orders/${orderId}/photos`, form);
  }

  removePhoto(orderId: string, photoId: string): Observable<OrderDto> {
    return this.http.delete<OrderDto>(`${BASE}/orders/${orderId}/photos/${photoId}`);
  }

  getOrder(orderId: string): Observable<OrderDto> {
    return this.http.get<OrderDto>(`${BASE}/orders/${orderId}`);
  }

  listOrders(): Observable<OrderSummaryDto[]> {
    return this.http.get<OrderSummaryDto[]>(`${BASE}/orders`);
  }

  saveSheetPlan(orderId: string, items: SheetPlanItem[]): Observable<OrderDto> {
    return this.http.put<OrderDto>(`${BASE}/orders/${orderId}/sheet-plan`, { items });
  }

  addDate(orderId: string, day: number, month: number, label: string): Observable<OrderDto> {
    return this.http.post<OrderDto>(`${BASE}/orders/${orderId}/dates`, { day, month, label });
  }

  removeDate(orderId: string, dateId: string): Observable<OrderDto> {
    return this.http.delete<OrderDto>(`${BASE}/orders/${orderId}/dates/${dateId}`);
  }

  saveHolidaySettings(orderId: string, countries: string[], weekStart: string): Observable<OrderDto> {
    return this.http.put<OrderDto>(`${BASE}/orders/${orderId}/holiday-settings`, { countries, weekStart });
  }

  startGeneration(orderId: string): Observable<OrderDto> {
    return this.http.post<OrderDto>(`${BASE}/orders/${orderId}/generate`, {});
  }

  // The single generation trigger (see #351) — used for a sheet's first variant and every one
  // after it; the server decides whether this one is free or costs a regeneration.
  generateSheet(
    orderId: string,
    index: number,
    promptId: string,
    imageStyleId: string,
    photoId?: string,
  ): Observable<OrderDto> {
    return this.http.post<OrderDto>(`${BASE}/orders/${orderId}/sheets/${index}/generate`, {
      promptId,
      imageStyleId,
      photoId: photoId || null,
    });
  }

  // Restores a previously generated variant as active — free, no generation involved.
  activateVariant(orderId: string, sheetId: string, variantId: string): Observable<OrderDto> {
    return this.http.post<OrderDto>(`${BASE}/orders/${orderId}/sheets/${sheetId}/variants/${variantId}/activate`, {});
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

  pay(orderId: string): Observable<{ pageUrl: string }> {
    return this.http.post<{ pageUrl: string }>(`${BASE}/orders/${orderId}/pay`, {});
  }

  setPrintQuantity(orderId: string, quantity: number): Observable<OrderDto> {
    return this.http.put<OrderDto>(`${BASE}/orders/${orderId}/print-quantity`, { quantity });
  }

  checkoutBatch(
    orderIds: string[],
    delivery: { recipientName: string; phone: string; city: string; warehouseNumber: string; warehouseAddress: string },
  ): Observable<void> {
    return this.http.post<void>(`${BASE}/orders/checkout-batch`, { orderIds, ...delivery });
  }

  payBatch(orderIds: string[]): Observable<{ pageUrl: string }> {
    return this.http.post<{ pageUrl: string }>(`${BASE}/orders/pay-batch`, orderIds);
  }

  cancel(orderId: string): Observable<OrderDto> {
    return this.http.post<OrderDto>(`${BASE}/orders/${orderId}/cancel`, {});
  }

  archiveOrder(orderId: string): Observable<void> {
    return this.http.post<void>(`${BASE}/orders/${orderId}/archive`, {});
  }

  unarchiveOrder(orderId: string): Observable<void> {
    return this.http.post<void>(`${BASE}/orders/${orderId}/unarchive`, {});
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
