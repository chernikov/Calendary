import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { SummaryOrder } from '../models/summary-order';
import { PaymentRedirect } from '../models/payment.redirect';

@Injectable({
  providedIn: 'root'
})
export class PaymentService {
  private apiUrl = '/api/pay';

  constructor(private http: HttpClient) {}

  getPay(): Observable<PaymentRedirect> {
    return this.http.get<PaymentRedirect>(`${this.apiUrl}`);
  }
}