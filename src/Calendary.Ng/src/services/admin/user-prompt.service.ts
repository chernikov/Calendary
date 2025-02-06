import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { CreateSynthesis, Synthesis } from '../../models/synthesis';
import { Prompt } from '../../models/prompt';

@Injectable({
  providedIn: 'root'
})
export class UserPromptService {
  private apiUrl = `/api/admin/user`;

  constructor(private http: HttpClient) {}

  getPromptByTrainingId(userId: number, trainingId: number): Observable<Prompt[]> {
    const url = `${this.apiUrl}/${userId}/prompt/${trainingId}`;
    return this.http.get<Prompt[]>(url);
  }
}
