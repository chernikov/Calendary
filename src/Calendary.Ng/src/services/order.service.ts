import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { SummaryOrder } from '../models/summary-order';
import { OrderResult } from '../models/results/order.result';
import { Order } from '../models/order';

@Injectable({
  providedIn: 'root'
})
export class OrderService {
  private apiUrl = '/api/order';

  constructor(private http: HttpClient) {}

  getOrderById(orderId: number): Observable<SummaryOrder> {
    return this.http.get<SummaryOrder>(`${this.apiUrl}/${orderId}`);
  }

  getUserOrders(page : number): Observable<OrderResult> {
    return this.http.get<OrderResult>(`${this.apiUrl}/my?page=${page}`);
  }

  updateComment(order: Order): Observable<Order> {
    return this.http.put<Order>(`${this.apiUrl}/comment`, order);
  }
}