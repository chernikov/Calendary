import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { OrderResult } from '../models/results/order.result';
import { AdminOrderResult } from '../models/results/admin-order.result';

@Injectable({
  providedIn: 'root'
})
export class AdminOrderService {
  private apiUrl = '/api/admin/order';

  constructor(private http: HttpClient) {}

  getOrders(page : number, pageSize : number): Observable<AdminOrderResult> {
    return this.http.get<AdminOrderResult>(`${this.apiUrl}?page=${page}&pageSize=${pageSize}`);
  }

  updateOrderStatus(orderId: number, status: string): Observable<any> {
    return this.http.put(`/api/admin/order/${orderId}/status`, { status });
  }
}