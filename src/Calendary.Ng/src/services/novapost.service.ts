import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { NovaPostItem } from '../models/nova-post-item';

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
}
