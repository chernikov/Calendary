import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Calendar } from '../../models/calendar';


@Injectable({
  providedIn: 'root',
})
export class CalendarService {
  private apiUrl = '/api/admin/user';

  constructor(private http: HttpClient) {}

  // GET /api/admin/user/{userId}/calendars
  getByUserId(userId: number): Observable<Calendar[]> {
    return this.http.get<Calendar[]>(`${this.apiUrl}/${userId}/calendars`);
  }
}