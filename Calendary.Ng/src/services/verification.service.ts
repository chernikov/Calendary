import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Verification } from '../models/verification';

@Injectable({
  providedIn: 'root',
})
export class VerificationService {
  private apiUrl = '/api/verification'; // Базова URL для API

  constructor(private http: HttpClient) {}

  //Відправка коду по email
  sendEmailVerification(): Observable<any> {
    return this.http.post(`${this.apiUrl}/send-email-verification`, null);
  }

  verifyEmailCode(code: string): Observable<any> {
    let verification = new Verification();
    verification.verificationCode = code;
    return this.http.post(`${this.apiUrl}/verify-email-code`, verification);
  }
}
