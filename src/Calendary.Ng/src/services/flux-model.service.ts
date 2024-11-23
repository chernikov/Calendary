import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { FluxModel } from '../models/flux-model';

@Injectable({
  providedIn: 'root'
})
export class FluxModelService {
  private readonly apiUrl = '/api/flux-model';

  constructor(private http: HttpClient) {}

  // взяти поточну модель FluxModel
  current(): Observable<FluxModel> {
    return this.http.get<FluxModel>(this.apiUrl);
  }
  // Створення нового FluxModel
  create(gender: string): Observable<FluxModel> {
    return this.http.post<FluxModel>(this.apiUrl, { gender });
  }

  // Отримання FluxModel за ID
  getById(id: number): Observable<FluxModel> {
    return this.http.get<FluxModel>(`${this.apiUrl}/${id}`);
  }

  // Оновлення статусу FluxModel
  updateStatus(id: number, status: string): Observable<FluxModel> {
    return this.http.patch<FluxModel>(`${this.apiUrl}/${id}`, status, {
      headers: { 'Content-Type': 'application/json' }
    });
  }
}