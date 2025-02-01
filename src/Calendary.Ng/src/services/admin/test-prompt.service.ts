import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AdminTestPrompt } from '../../models/admin-test-prompt';
import { CreateTestPrompt } from '../../models/create-test-prompt';

@Injectable({
  providedIn: 'root',
})
export class AdminTestPromptService {
  private baseUrl = `/api/admin/test-prompt`;

  constructor(private http: HttpClient) {}

  // Отримати список тестів для конкретного промпту
  getByPromptId(promptId: number): Observable<AdminTestPrompt[]> {
    return this.http.get<AdminTestPrompt[]>(`${this.baseUrl}/${promptId}`);
  }

  // Створити новий тест з використанням нового DTO
  create(testPrompt: CreateTestPrompt): Observable<AdminTestPrompt> {
    return this.http.post<AdminTestPrompt>(`${this.baseUrl}`, testPrompt);
  }

  // Виконати тест
  runTestPrompt(id: number): Observable<AdminTestPrompt> {
    return this.http.get<AdminTestPrompt>(`${this.baseUrl}/run/${id}`);
  }
}
