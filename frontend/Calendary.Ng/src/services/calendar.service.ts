import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Calendar } from '../models/calendar'; // Модель Calendar
import { FillCalendar } from '../models/requests/fill-calendar';

@Injectable({
  providedIn: 'root'
})
export class CalendarService {
  private apiUrl = '/api/calendar'; // Базова URL для API
  private apiGenerateUrl = '/api/calendar/generate'; // URL для API генерації PDF

  private apiAddToCartUrl = '/api/calendar/add-cart'; // URL для API додавання в кошик

  constructor(private http: HttpClient) { }

  // Створення нового календаря
  createCalendar(calendar: Calendar): Observable<Calendar> {
    return this.http.post<Calendar>(this.apiUrl, calendar);
  }

  // Отримання поточного календаря
  getCalendar(): Observable<Calendar> {
    return this.http.get<Calendar>(`${this.apiUrl}`);
  }

  // Оновлення календаря
  updateCalendar(calendar: Calendar): Observable<Calendar> {
    return this.http.put<Calendar>(`${this.apiUrl}`, calendar);
  }


  generatePdf(calendarId : number, fluxModelId : number | null) : Observable<Calendar> 
  {
    if (fluxModelId) {
      return this.http.get<Calendar>(`${this.apiGenerateUrl}/${calendarId}?fluxModelId=${fluxModelId}`);
    } else {
      return this.http.get<Calendar>(`${this.apiGenerateUrl}/${calendarId}`);
    }
  }

  addToCart(calendar: Calendar): Observable<Calendar> {
    return this.http.post<Calendar>(`${this.apiAddToCartUrl}/${calendar.id}`, null);
  }

  fill(fillCalendar : FillCalendar) : Observable<any> {
    return this.http.post(`${this.apiUrl}/fill`, fillCalendar);
  }
}