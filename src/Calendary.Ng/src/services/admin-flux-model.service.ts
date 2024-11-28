import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AdminFluxModel } from '../models/admin-flux-model';

@Injectable({
  providedIn: 'root',
})
export class AdminFluxModelService {
  private apiUrl = '/api/admin/fluxmodels';

  constructor(private http: HttpClient) {}

  getAll(page: number = 1, pageSize: number = 10): Observable<AdminFluxModel[]> {
    const params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());

    return this.http.get<AdminFluxModel[]>(this.apiUrl, { params });
  }

  getById(id: number): Observable<AdminFluxModel> {
    return this.http.get<AdminFluxModel>(`${this.apiUrl}/${id}`);
  }
}