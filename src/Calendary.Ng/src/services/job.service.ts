import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Job } from '../models/job';

@Injectable({
  providedIn: 'root',
})
export class JobService {
  private apiUrl = '/api/job';

  constructor(private http: HttpClient) {}

  /**
   * Генерує Default Job для FluxModel.
   * @param fluxModelId ID FluxModel
   * @returns Observable<Job>
   */
  createDefaultJob(fluxModelId: number): Observable<Job> {
    return this.http.get<Job>(`${this.apiUrl}/default/${fluxModelId}`);
  }


    /**
   * Генерує Job для FluxModel.
   * @param fluxModelId ID FluxModel
   * @param promptThemeId ID теми
   * @returns Observable<Job>
   */
    createJob(fluxModelId: number, promptThemeId: number): Observable<Job> {
        return this.http.get<Job>(`${this.apiUrl}/generate/${fluxModelId}?promptThemeId=${promptThemeId}`);
      }

  /**
   * Виконує Job з усіма JobTasks.
   * @param jobId ID Job
   * @returns Observable<string>
   */
  runJob(jobId: number): Observable<string> {
    return this.http.get<string>(`${this.apiUrl}/run/${jobId}`);
  }

   /**
   * Додає до черги Job з усіма JobTasks.
   * @param jobId ID Job
   * @returns Observable<string>
   */
   messageJob(jobId: number): Observable<string> {
    return this.http.get<string>(`${this.apiUrl}/message/${jobId}`);
  }
}