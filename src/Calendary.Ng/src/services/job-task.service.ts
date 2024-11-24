import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { JobTask } from '../models/job-task';

@Injectable({
  providedIn: 'root',
})
export class JobTaskService {
  private apiUrl = '/api/job-task';

  constructor(private http: HttpClient) {}

  run(taskId: number): Observable<JobTask> {
    return this.http.get<JobTask>(`${this.apiUrl}/run/${taskId}`);
  }
}