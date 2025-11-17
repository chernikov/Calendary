import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Category } from '../../models/category';

@Injectable({
  providedIn: 'root',
})
export class AdminCategoryService {
  private apiUrl = '/api/admin/category';

  constructor(private http: HttpClient) {}

  // Отримати всі категорії
  getAll(): Observable<Category[]> {
    return this.http.get<Category[]>(this.apiUrl);
  }

  // Отримати категорію за ID
  getById(id: number): Observable<Category> {
    return this.http.get<Category>(`${this.apiUrl}/${id}`);
  }

  // Створити нову категорію
  create(category: Category): Observable<Category> {
    return this.http.post<Category>(this.apiUrl, category);
  }

  // Оновити існуючу категорію
  update(category: Category): Observable<Category> {
    return this.http.put<Category>(this.apiUrl, category);
  }

  // Видалити категорію за ID
  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}