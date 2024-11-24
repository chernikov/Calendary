import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class TrainingService {
  private apiUrl = '/api/training'; // Базовий URL для API

  constructor(private http: HttpClient) {}

  getStatus(trainingId: number): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/${trainingId}`);
  }

  cancelTraining(trainingId: number): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/${trainingId}/cancel`, {});
  }
}