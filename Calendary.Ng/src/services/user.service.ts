import { Observable } from 'rxjs';
import { User, UserInfo, UserLogin } from '../models/user';
import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class UserService {
  private apiUrl = '/api/user'; // Базовий URL API

  constructor(private http: HttpClient) {}

  // Метод для реєстрації користувача
  register(user: UserLogin): Observable<any> {
    return this.http.post(`${this.apiUrl}/register`, user);
  }

  login(user: UserLogin): Observable<any> {
    return this.http.post(`${this.apiUrl}/login`, user);
  }

  getInfo() :Observable<UserInfo> {
    return this.http.get<UserInfo>(`${this.apiUrl}`);
  }

  updateInfo(userInfo: UserInfo) :Observable<UserInfo> {
    return this.http.put<UserInfo>(`${this.apiUrl}`, userInfo);
  }
}
