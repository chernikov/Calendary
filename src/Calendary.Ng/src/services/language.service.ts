// services/language.service.ts
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Language } from '../models/language'; // Модель для мови

@Injectable({
  providedIn: 'root',
})
export class LanguageService {
  private apiUrl = '/api/language'; // API для отримання списку мов

  constructor(private http: HttpClient) {}

  // Метод для отримання списку мов
  getLanguages(): Observable<Language[]> {
    return this.http.get<Language[]>(this.apiUrl);
  }
}