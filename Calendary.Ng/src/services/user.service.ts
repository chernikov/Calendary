import { Observable } from "rxjs";
import { User } from "../models/user";
import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";


@Injectable({
    providedIn: 'root'
  })
export class UserService {

    private apiUrl = '/api/users'; // Базовий URL API
  
    constructor(private http: HttpClient) { }
  
    // Метод для реєстрації користувача
    register(user: User): Observable<any> {
      return this.http.post(`${this.apiUrl}/register`, user);
    }

    login(user: User): Observable<any> {
        return this.http.post(`${this.apiUrl}/login`, user);
      }
  }