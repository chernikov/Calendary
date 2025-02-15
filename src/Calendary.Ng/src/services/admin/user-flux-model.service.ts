import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AdminFluxModel, CreateFluxModel, FluxModel } from '../../models/flux-model';
import { Photo } from '../../models/photo';
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

  // GET /api/admin/flux-models/{fluxModelId}
  getById( fluxModelId: number): Observable<AdminFluxModel> {
    return this.http.get<AdminFluxModel>(`/api/admin/fluxmodels/${fluxModelId}`);
  }
  
  delete(userId: number, fluxModelId: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${userId}/flux-models/${fluxModelId}`);
  }

   // Додаємо метод для отримання фотографій, пов'язаних із flux моделлю
   getPhotos(userId: number, fluxModelId: number): Observable<Photo[]> {
    const url = `${this.apiUrl}/${userId}/flux-models/${fluxModelId}/photos`;
    return this.http.get<Photo[]>(url);
  }

  changeName(userId: number, updatedModel: FluxModel): Observable<void> {
    const url = `${this.apiUrl}/${userId}/flux-models/change-name`;
    return this.http.put<void>(url, updatedModel);
  }

  create(userId : number, model: CreateFluxModel): Observable<FluxModel> {
    const url = `${this.apiUrl}/${userId}/flux-models`;
    return this.http.post<FluxModel>(url, model);
  }
}