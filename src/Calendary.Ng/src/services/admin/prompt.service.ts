import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Prompt } from '../../models/prompt';
import { PromptSeed } from '../../models/promt-seed';

@Injectable({
  providedIn: 'root',
})
export class AdminPromptService {
  private apiUrl = '/api/admin/prompt';

  constructor(private http: HttpClient) {}

  getAll(
    themeId: number | null,
    categoryId: number | null
  ): Observable<Prompt[]> {
    let url = this.apiUrl;
    const params: string[] = [];

    // Додаємо параметри, якщо вони є
    if (themeId !== null && themeId !== undefined) {
      params.push(`themeId=${themeId}`);
    }
    if (categoryId !== null && categoryId !== undefined) {
      params.push(`categoryId=${categoryId}`);
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

  create(prompt: Prompt): Observable<Prompt> {
    return this.http.post<Prompt>(`${this.apiUrl}`, prompt);
  }

  update(prompt: Prompt): Observable<Prompt> {
    return this.http.put<Prompt>(`${this.apiUrl}`, prompt);
  }

  assignSeed(promptSeed: PromptSeed): Observable<PromptSeed> {
    return this.http.post<PromptSeed>(`${this.apiUrl}/assign`, promptSeed);
  }

  deassignSeed(promptSeed: PromptSeed): Observable<PromptSeed> {
    return this.http.post<PromptSeed>(`${this.apiUrl}/deassign`, promptSeed);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
