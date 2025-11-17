import { Observable } from 'rxjs';
import { User, UserInfo, UserLogin, UserRegister } from '../models/user';
import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { ChangePassword } from '../models/change-password';
import { ForgotPassword } from '../models/forgot-password';
import { NewPassword } from '../models/new-password';

@Injectable({
  providedIn: 'root',
})
export class UserService {
  private apiUrl = '/api/user'; // Базовий URL API

  constructor(private http: HttpClient) {}

  // Метод для реєстрації користувача
  register(user: UserRegister): Observable<any> {
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

  changePassword(changePassword: ChangePassword) :Observable<any> {
    return this.http.post(`${this.apiUrl}/change-password`, changePassword);
  }

  newPassword(newPassword: NewPassword) :Observable<any> {
    return this.http.post(`${this.apiUrl}/new-password`, newPassword);
  }

  sendResetLink(forgotPassword: ForgotPassword): Observable<any> {
    return this.http.post(`${this.apiUrl}/forgot-password`, forgotPassword);
  }

  verify(token: string): Observable<User> {
    return this.http.get<User>(`${this.apiUrl}/verify?token=${token}`);
  }
}
