import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Prompt } from '../models/prompt';

@Injectable({
  providedIn: 'root',
})
export class AdminPromptService {
  private apiUrl = '/api/admin/prompt';

  constructor(private http: HttpClient) {}

  getAll(
    themeId: number | null,
    ageGender: number | null
  ): Observable<Prompt[]> {
    let url = this.apiUrl;
    const params: string[] = [];

    // Додаємо параметри, якщо вони є
    if (themeId !== null && themeId !== undefined) {
      params.push(`themeId=${themeId}`);
    }
    if (ageGender !== null && ageGender !== undefined) {
      params.push(`ageGender=${ageGender}`);
    }

    // Формуємо URL з параметрами, якщо вони є
    if (params.length > 0) {
      url += `?${params.join('&')}`;
    }

    return this.http.get<Prompt[]>(url);
  }

  getById(id: number): Observable<Prompt> {
    return this.http.get<Prompt>(`${this.apiUrl}/${id}`);
  }

  create(Prompt: Prompt): Observable<Prompt> {
    return this.http.post<Prompt>(`${this.apiUrl}`, Prompt);
  }

  update(Prompt: Prompt): Observable<Prompt> {
    return this.http.put<Prompt>(`${this.apiUrl}`, Prompt);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
