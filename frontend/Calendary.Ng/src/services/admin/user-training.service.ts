import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Training } from '../../models/training';

@Injectable({
  providedIn: 'root'
})
export class UserTrainingService {
  // Базовий URL для адмінських endpoint-ів користувача
  private apiUrl = `/api/admin/user`;

  constructor(private http: HttpClient) {}


  get(userId: number, trainingId: number): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/${userId}/training/${trainingId}`);
  }
  
  /**
   * Викликає endpoint для генерації тренування для заданої flux моделі.
   * URL форматується як: /api/admin/user/{userId}/training/generate/{fluxModelId}
   *
   * @param userId ідентифікатор користувача
   * @param fluxModelId ідентифікатор flux моделі, для якої потрібно згенерувати тренування
   * @returns Observable з результатом виконання запиту
   */
  generateTraining(userId: number, fluxModelId: number): Observable<any> 
  {
    const url = `${this.apiUrl}/${userId}/training/generate/${fluxModelId}`;
    return this.http.get<any>(url);
  }

  getStatus(userId: number, trainingId: number): Observable<Training> {
    const url = `${this.apiUrl}/${userId}/training/status/${trainingId}`;
    return this.http.get<any>(url);
  }
}
