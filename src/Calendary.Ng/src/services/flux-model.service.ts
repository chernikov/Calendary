import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
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
  create(ageGender: number): Observable<FluxModel> {
    return this.http.post<FluxModel>(this.apiUrl, { ageGender });
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

  generate(id : number) : Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/generate`, { id });
  }

  archive(id : number) : Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/archive/${id}`, null);
  }
}