import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { FluxModel } from '../../models/flux-model';

@Injectable({
  providedIn: 'root'
})
export class UserFluxModelService {
  // Базовий URL API; при необхідності замініть на відповідний шлях або використовуйте environment
  private apiUrl = '/api/admin/user';

  constructor(private http: HttpClient) {}

  // GET /api/admin/user/{userId}/flux-models
  getUserFluxModels(userId: number): Observable<FluxModel[]> {
    return this.http.get<FluxModel[]>(`${this.apiUrl}/${userId}/flux-models`);
  }

  
  delete(userId: number, fluxModelId: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${userId}/flux-models/${fluxModelId}`);
  }
}