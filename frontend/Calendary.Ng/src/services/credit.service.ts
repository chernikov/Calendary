import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../environments/environment';

export interface CreditBalance {
  total: number;
  purchased: number;
  bonus: number;
  expiringIn30Days: number;
}

export interface CreditPackage {
  id: number;
  name: string;
  credits: number;
  bonusCredits: number;
  priceUAH: number;
  description?: string;
  displayOrder: number;
}

export interface CreditTransaction {
  id: number;
  amount: number;
  type: string;
  description: string;
  createdAt: Date;
  order?: { id: number; orderDate: Date };
  fluxModel?: { id: number; name: string };
  creditPackage?: { id: number; name: string };
}

export interface PurchaseResponse {
  paymentUrl: string;
  package: {
    id: number;
    name: string;
    credits: number;
    bonusCredits: number;
    priceUAH: number;
  };
}

@Injectable({
  providedIn: 'root'
})
export class CreditService {
  private apiUrl = `${environment.apiUrl}/api/credits`;

  constructor(private http: HttpClient) {}

  /**
   * Отримати баланс кредитів
   */
  getBalance(): Observable<CreditBalance> {
    return this.http.get<CreditBalance>(`${this.apiUrl}/balance`);
  }

  /**
   * Отримати доступні пакети кредитів
   */
  getPackages(): Observable<CreditPackage[]> {
    return this.http.get<CreditPackage[]>(`${this.apiUrl}/packages`);
  }

  /**
   * Купити пакет кредитів
   */
  purchasePackage(packageId: number): Observable<PurchaseResponse> {
    return this.http.post<PurchaseResponse>(`${this.apiUrl}/purchase`, { packageId });
  }

  /**
   * Отримати історію транзакцій
   */
  getTransactions(skip: number = 0, take: number = 50): Observable<CreditTransaction[]> {
    return this.http.get<CreditTransaction[]>(`${this.apiUrl}/transactions`, {
      params: { skip: skip.toString(), take: take.toString() }
    });
  }

  /**
   * Перевірити чи достатньо кредитів
   */
  checkBalance(amount: number): Observable<{
    required: number;
    available: number;
    hasEnough: boolean;
    shortfall: number;
  }> {
    return this.http.get<any>(`${this.apiUrl}/check`, {
      params: { amount: amount.toString() }
    });
  }
}
