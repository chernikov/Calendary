import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AdminSynthesis } from '../../models/admin-synthesis';
import { CreateSynthesis } from '../../models/create-synthesis';

@Injectable({
  providedIn: 'root',
})
export class AdminSynthesisService {
  private baseUrl = `/api/admin/synthesis`;

  constructor(private http: HttpClient) {}

  // Отримати список тестів для конкретного промпту
  getByPromptId(promptId: number): Observable<AdminSynthesis[]> {
    return this.http.get<AdminSynthesis[]>(`${this.baseUrl}/${promptId}`);
  }

  // Створити новий тест з використанням нового DTO
  create(synthesis: CreateSynthesis): Observable<AdminSynthesis> {
    return this.http.post<AdminSynthesis>(`${this.baseUrl}`, synthesis);
  }

  // Виконати тест
  runSynthesis(id: number): Observable<AdminSynthesis> {
    return this.http.get<AdminSynthesis>(`${this.baseUrl}/run/${id}`);
  }
}
