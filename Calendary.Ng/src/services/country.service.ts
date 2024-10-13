// services/country.service.ts
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Country } from '../models/country'; // Модель для країни

@Injectable({
  providedIn: 'root',
})
export class CountryService {
  private apiUrl = '/api/country'; // API для отримання списку країн

  constructor(private http: HttpClient) {}

  // Метод для отримання списку країн
  getCountries(): Observable<Country[]> {
    return this.http.get<Country[]>(this.apiUrl);
  }
}