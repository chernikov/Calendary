import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { EventDate } from '../models/event-date'; // Модель даних EventDate

@Injectable({
  providedIn: 'root'
})
export class EventDateService {
  private apiUrl = `api/eventdates`; // Базова URL для API

  constructor(private http: HttpClient) { }

  // Отримати всі EventDates для користувача
  getAll(): Observable<EventDate[]> {
    return this.http.get<EventDate[]>(`${this.apiUrl}`);
  }

  getEventDate(id: number): Observable<EventDate> {
    return this.http.get<EventDate>(`${this.apiUrl}/${id}`);
  }

  createEventDate(eventDate: EventDate): Observable<EventDate> {
    return this.http.post<EventDate>(this.apiUrl, eventDate);
  }

  updateEventDate(eventDate: EventDate): Observable<EventDate> {
    return this.http.put<EventDate>(`${this.apiUrl}`, eventDate);
  }

  deleteEventDate(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
