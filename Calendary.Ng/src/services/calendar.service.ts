import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Calendar } from '../models/calendar'; // Модель Calendar

@Injectable({
  providedIn: 'root'
})
export class CalendarService {
  private apiUrl = '/api/calendar'; // Базова URL для API
  private apiGenerateUrl = '/api/calendar/generate'; // URL для API генерації PDF

  constructor(private http: HttpClient) { }

  // Створення нового календаря
  createCalendar(calendar: Calendar): Observable<Calendar> {
    return this.http.post<Calendar>(this.apiUrl, calendar);
  }

  // Отримання календаря за ID
  getCalendar(): Observable<Calendar> {
    return this.http.get<Calendar>(`${this.apiUrl}`);
  }

  // Оновлення календаря
  updateCalendar(calendar: Calendar): Observable<Calendar> {
    return this.http.put<Calendar>(`${this.apiUrl}`, calendar);
  }


  generatePdf(calendarId : number) : Observable<Calendar> {
    return this.http.get<Calendar>(`${this.apiGenerateUrl}/${calendarId}`);
  }
}