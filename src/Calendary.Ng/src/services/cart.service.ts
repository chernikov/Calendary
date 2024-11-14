import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Calendar } from '../models/calendar'; // Модель Calendar
import { Order } from '../models/order';
import { OrderItem } from '../models/order-item';
import { DeliveryInfo } from '../models/delivery-info';
import { SummaryOrder } from '../models/summary-order';

@Injectable({
  providedIn: 'root'
})
export class CartService {
  private apiUrl = '/api/cart'; // Базова URL для API
  
  constructor(private http: HttpClient) { }

  // Отримання кошика
  getCart(): Observable<Order> {
    return this.http.get<Order>(this.apiUrl);
  }

  deleteItem(itemId: number): Observable<any> {
    return this.http.delete<any>(`${this.apiUrl}/${itemId}`);
  }

  updateItem(item : OrderItem) : Observable<any> {
    return this.http.put<OrderItem>(`${this.apiUrl}/item`, item);
  }

  itemsInCart() : Observable<number> {
    return this.http.get<number>(`${this.apiUrl}/count`);
  }

  // Метод для отримання інформації про доставку
  getDeliveryInfo(): Observable<DeliveryInfo> {
    return this.http.get<DeliveryInfo>(`${this.apiUrl}/delivery`);
  }

  // Метод для оновлення інформації про доставку
  updateDeliveryInfo(delivery: DeliveryInfo): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/delivery`, delivery);
  }

  summary() : Observable<SummaryOrder> {
    return this.http.get<SummaryOrder>(`${this.apiUrl}/summary`);
  }
}