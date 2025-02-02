import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Synthesis } from '../../models/synthesis';

@Injectable({
  providedIn: 'root'
})
export class UserSynthesisService {
  private apiUrl = `/api/admin/user`;

  constructor(private http: HttpClient) {}

  /**
   * Отримує synthesis за trainingId для заданого користувача.
   * Формує маршрут:
   * /api/admin/user/{userId}/synthesis/{trainingId}
   *
   * @param userId - ідентифікатор користувача
   * @param trainingId - ідентифікатор тренування
   * @returns Observable з масивом synthesis
   */
  getSynthesisesByTrainingId(userId: number, trainingId: number): Observable<Synthesis[]> {
    const url = `${this.apiUrl}/${userId}/synthesis/${trainingId}`;
    return this.http.get<Synthesis[]>(url);
  }
}
