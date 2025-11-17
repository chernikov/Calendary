import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { PromptTheme } from '../../models/prompt-theme';

@Injectable({
    providedIn: 'root'
})
export class AdminPromptThemeService {
    private apiUrl = '/api/admin/prompt-theme';

    constructor(private http: HttpClient) {}

    getAll(): Observable<PromptTheme[]> {
        return this.http.get<PromptTheme[]>(`${this.apiUrl}`);
    }

    getById(id: number): Observable<PromptTheme> {
        return this.http.get<PromptTheme>(`${this.apiUrl}/${id}`);
    }

    create(promptTheme: PromptTheme): Observable<PromptTheme> {
        return this.http.post<PromptTheme>(`${this.apiUrl}`, promptTheme);
    }

    update(promptTheme: PromptTheme): Observable<PromptTheme> {
        return this.http.put<PromptTheme>(`${this.apiUrl}`, promptTheme);
    }

    delete(id: number): Observable<void> {
        return this.http.delete<void>(`${this.apiUrl}/${id}`);
    }
}