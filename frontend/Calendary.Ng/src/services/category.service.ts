import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Category } from '../models/category';

@Injectable({
  providedIn: 'root',
})
export class CategoryService {
  private apiUrl = '/api/category';

  constructor(private http: HttpClient) {}

  // Отримати всі категорії
  getAll(): Observable<Category[]> {
    return this.http.get<Category[]>(this.apiUrl);
  }
}