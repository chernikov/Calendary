import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { NovaPostItem } from '../models/nova-post-item';

export interface DeliveryCalculationRequest {
  recipientCityRef: string;
  weight: number;
  declaredValue: number;
}

export interface DeliveryCostResponse {
  cost: number;
}

export interface CreateTTNRequest {
  recipientCityRef: string;
  recipientWarehouseRef: string;
  recipientName: string;
  recipientPhone: string;
  weight: number;
  declaredValue: number;
  description: string;
}

export interface TTNResponse {
  trackingNumber: string;
}

@Injectable({
  providedIn: 'root',
})
export class NovaPostService {
  private apiUrl = 'api/nova-search';

  constructor(private http: HttpClient) {}

  searchCity(query: string): Observable<NovaPostItem[]> {
    return this.http.get<NovaPostItem[]>(`${this.apiUrl}/city`, {
      params: { search: query },
    });
  }

  searchWarehouse(city: string, query: string): Observable<NovaPostItem[]> {
    return this.http.get<NovaPostItem[]>(`${this.apiUrl}/warehouse`, {
      params: { city, search: query },
    });
  }

  calculateDeliveryCost(
    request: DeliveryCalculationRequest
  ): Observable<DeliveryCostResponse> {
    return this.http.post<DeliveryCostResponse>(
      `${this.apiUrl}/calculate-delivery`,
      request
    );
  }

  createTTN(request: CreateTTNRequest): Observable<TTNResponse> {
    return this.http.post<TTNResponse>(`${this.apiUrl}/create-ttn`, request);
  }
}
