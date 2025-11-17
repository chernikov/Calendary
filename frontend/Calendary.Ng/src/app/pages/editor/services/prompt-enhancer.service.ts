import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface PromptEnhanceRequest {
  prompt: string;
}

export interface PromptEnhanceResponse {
  enhancedPrompt: string;
  suggestions: string[];
}

@Injectable({
  providedIn: 'root'
})
export class PromptEnhancerService {
  private apiUrl = '/api/prompts';

  constructor(private http: HttpClient) {}

  enhancePrompt(prompt: string): Observable<PromptEnhanceResponse> {
    const request: PromptEnhanceRequest = { prompt };
    return this.http.post<PromptEnhanceResponse>(`${this.apiUrl}/enhance`, request);
  }
}
