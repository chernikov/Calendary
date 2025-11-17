import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';


@Injectable({
  providedIn: 'root'
})
export class TrainingService {

  private baseUrl = `/api/admin/trainings`;

  constructor(private http: HttpClient) { }

  /**
   * Виконує soft‑видалення тренування.
   * Повертає Observable<void>, оскільки сервер повертає No Content.
   *
   * @param trainingId - ідентифікатор тренування для soft‑видалення
   */
  softDelete(trainingId: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${trainingId}`);
  }

  // Можна додати інші методи для роботи з тренуваннями,
  // наприклад, отримання списку, створення, оновлення і т.д.
}
