import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Image } from "../models/image";
@Injectable({
    providedIn: 'root'
})
export class ImageService {

    private baseUrl: string = '/api/image';

    constructor(private http: HttpClient) {}

    getImages(calendarId: number): Observable<Image[]> {
        return this.http.get<Image[]>(`${this.baseUrl}/${calendarId}`);
    }
}