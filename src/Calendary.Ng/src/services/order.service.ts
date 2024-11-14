import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { SummaryOrder } from '../models/summary-order';

@Injectable({
  providedIn: 'root'
})
export class OrderService {
  private apiUrl = '/api/order';

  constructor(private http: HttpClient) {}

  getOrderById(orderId: number): Observable<SummaryOrder> {
    return this.http.get<SummaryOrder>(`${this.apiUrl}/${orderId}`);
  }
}