import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { HolidayPreset, ApplyPresetRequest } from '../models/holiday-preset';

@Injectable({
  providedIn: 'root',
})
export class HolidayPresetService {
  private apiUrl = '/api/holiday-presets';

  constructor(private http: HttpClient) {}

  getAllPresets(): Observable<HolidayPreset[]> {
    return this.http.get<HolidayPreset[]>(this.apiUrl);
  }

  getPresetByCode(code: string): Observable<HolidayPreset> {
    return this.http.get<HolidayPreset>(`${this.apiUrl}/${code}`);
  }

  getPresetsByType(type: string): Observable<HolidayPreset[]> {
    return this.http.get<HolidayPreset[]>(`${this.apiUrl}/by-type/${type}`);
  }

  applyPresetToCalendar(request: ApplyPresetRequest): Observable<boolean> {
    return this.http.post<boolean>(`${this.apiUrl}/apply`, request);
  }
}
