import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Holiday } from '../models/holiday';
import { User } from '../models/user';

@Injectable({
    providedIn: 'root',
  })
  export class TempUserService {
    private apiUrl = '/api/tempuser'; // URL до твого API
  
    constructor(private http: HttpClient) {}
  
    init(): Observable<User | null> {
      return this.http.post<User | null>(this.apiUrl + '/init', {});
    }
  }