import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { PromptTheme } from '../models/prompt-theme';

@Injectable({
    providedIn: 'root'
})
export class PromptThemeService {
    private apiUrl = '/api/prompt-theme';

    constructor(private http: HttpClient) {}

    getAll(): Observable<PromptTheme[]> {
        return this.http.get<PromptTheme[]>(`${this.apiUrl}`);
    }
}