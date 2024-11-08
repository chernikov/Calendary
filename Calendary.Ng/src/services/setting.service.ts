// services/setting.service.ts
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Setting } from '../models/setting';

@Injectable({
  providedIn: 'root',
})
export class SettingService {
  private apiUrl = '/api/settings'; // URL до API

  constructor(private http: HttpClient) {}

  // Отримуємо налаштування для користувача
  getSettings(): Observable<Setting> {
    return this.http.get<Setting>(this.apiUrl);
  }

  // Зберігаємо налаштування
  saveSettings(settings: Setting): Observable<any> {
    return this.http.put(`${this.apiUrl}`, settings);
  }

  error() {
    return this.http.get(`/api/error`);
  }
}