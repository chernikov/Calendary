import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AdminUser, AdminCreateUser } from '../../models/admin-user';

@Injectable({
  providedIn: 'root',
})
export class AdminUserService {
  private apiUrl = '/api/admin/user';

  constructor(private http: HttpClient) {}

  // Отримати всіх користувачів
  getAll(): Observable<AdminUser[]> {
    return this.http.get<AdminUser[]>(this.apiUrl);
  }

  // Отримати користувача за ID
  getById(id: number): Observable<AdminUser> {
    return this.http.get<AdminUser>(`${this.apiUrl}/${id}`);
  }

  // Створити нову користувача
  create(user: AdminCreateUser): Observable<AdminUser> {
    return this.http.post<AdminUser>(this.apiUrl, user);
  }

  // Оновити існуючого користувача
  update(user: AdminUser): Observable<AdminUser> {
    return this.http.put<AdminUser>(this.apiUrl, user);
  }

  // Видалити користувача за ID
  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}