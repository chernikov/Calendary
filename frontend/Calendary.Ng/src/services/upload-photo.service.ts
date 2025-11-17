import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class UploadPhotoService {
  private apiUrl = '/api/upload-photo';

  constructor(private http: HttpClient) {}

  complete(fluxModelId : number): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/complete/${fluxModelId}`);
  }
}