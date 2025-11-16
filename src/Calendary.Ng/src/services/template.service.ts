import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  TemplateDetail,
  TemplatePagedResult,
  TemplateQueryParams,
  TemplateSummary,
} from '../models/template';

@Injectable({
  providedIn: 'root',
})
export class TemplateService {
  private readonly apiUrl = '/api/templates';

  constructor(private http: HttpClient) {}

  getTemplates(params: TemplateQueryParams): Observable<TemplatePagedResult> {
    let httpParams = new HttpParams();

    Object.entries(params).forEach(([key, value]) => {
      if (value === undefined || value === null || value === '') {
        return;
      }

      httpParams = httpParams.set(key, `${value}`);
    });

    return this.http.get<TemplatePagedResult>(this.apiUrl, { params: httpParams });
  }

  getTemplate(id: number): Observable<TemplateDetail> {
    return this.http.get<TemplateDetail>(`${this.apiUrl}/${id}`);
  }

  getCategories(): Observable<string[]> {
    return this.http.get<string[]>(`${this.apiUrl}/categories`);
  }

  getFeatured(count = 4): Observable<TemplateSummary[]> {
    const params = new HttpParams().set('count', `${count}`);
    return this.http.get<TemplateSummary[]>(`${this.apiUrl}/featured`, { params });
  }
}
