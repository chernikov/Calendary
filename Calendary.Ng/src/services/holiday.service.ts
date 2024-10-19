import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Holiday } from '../models/holiday';

@Injectable({
    providedIn: 'root',
  })
  export class HolidayService {
    private apiUrl = '/api/holiday'; // URL до твого API
  
    constructor(private http: HttpClient) {}
  
    getHolidays(): Observable<Holiday[]> {
      return this.http.get<Holiday[]>(this.apiUrl);
    }
  
    createHoliday(holiday: Holiday): Observable<Holiday> {
      return this.http.post<Holiday>(this.apiUrl, holiday);
    }
  
    updateHoliday(holiday: Holiday): Observable<Holiday> {
      return this.http.put<Holiday>(`${this.apiUrl}`, holiday);
    }
  
    deleteHoliday(id: number): Observable<void> {
      return this.http.delete<void>(`${this.apiUrl}/${id}`);
    }
  }