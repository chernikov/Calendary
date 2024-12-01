import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { PaymentRedirect } from '../models/payment-redirect';

@Injectable({
  providedIn: 'root'
})
export class PaymentService {
  private apiUrl = '/api/pay';

  constructor(private http: HttpClient) {}

  getPay(): Observable<PaymentRedirect> {
    return this.http.get<PaymentRedirect>(`${this.apiUrl}`);
  }

  getFluxModelPay(id : number): Observable<PaymentRedirect> {
    return this.http.get<PaymentRedirect>(`${this.apiUrl}/flux-model/${id}`);
  }
}