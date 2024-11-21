import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Prompt } from '../models/prompt';

@Injectable({
    providedIn: 'root'
})
export class AdminPromptService {
    private apiUrl = '/api/admin/prompt';

    constructor(private http: HttpClient) {}

    getAll(themeId : number | null): Observable<Prompt[]> {
        if (themeId) {
            return this.http.get<Prompt[]>(`${this.apiUrl}/?themeId=${themeId}`);
        } else {
            return this.http.get<Prompt[]>(`${this.apiUrl}`);
        }
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