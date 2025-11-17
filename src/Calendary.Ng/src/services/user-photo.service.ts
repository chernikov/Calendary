import { Injectable } from '@angular/core';
import { HttpClient, HttpEvent } from '@angular/common/http';
import { Observable } from 'rxjs';
import { UserPhoto } from '../models/user-photo';

@Injectable({
  providedIn: 'root'
})
export class UserPhotoService {
  private apiUrl = '/api/user-photo';

  constructor(private http: HttpClient) {}

  getUserPhotos(): Observable<UserPhoto[]> {
    return this.http.get<UserPhoto[]>(this.apiUrl);
  }

  getUserPhoto(id: number): Observable<UserPhoto> {
    return this.http.get<UserPhoto>(`${this.apiUrl}/${id}`);
  }

  uploadPhoto(file: File, caption?: string): Observable<{ message: string; photo: UserPhoto }> {
    const formData = new FormData();
    formData.append('file', file);
    if (caption) {
      formData.append('caption', caption);
    }

    return this.http.post<{ message: string; photo: UserPhoto }>(this.apiUrl, formData);
  }

  uploadPhotoWithProgress(file: File, caption?: string): Observable<HttpEvent<{ message: string; photo: UserPhoto }>> {
    const formData = new FormData();
    formData.append('file', file);
    if (caption) {
      formData.append('caption', caption);
    }

    return this.http.post<{ message: string; photo: UserPhoto }>(this.apiUrl, formData, {
      reportProgress: true,
      observe: 'events'
    });
  }

  updatePhoto(id: number, caption: string): Observable<{ message: string; photo: UserPhoto }> {
    return this.http.put<{ message: string; photo: UserPhoto }>(`${this.apiUrl}/${id}`, { caption });
  }

  deletePhoto(id: number): Observable<{ message: string }> {
    return this.http.delete<{ message: string }>(`${this.apiUrl}/${id}`);
  }
}
