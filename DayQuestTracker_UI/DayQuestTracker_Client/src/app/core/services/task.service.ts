import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environments';
import {
  HabitTask,
  DailyTaskView,
  CreateHabitTaskRequest,
  UpdateHabitTaskRequest,
  LogCompletionRequest
} from '../models/task.model';

@Injectable({ providedIn: 'root' })
export class TaskService {
  private readonly apiUrl = `${environment.apiUrl}`;

  constructor(private http: HttpClient) {}

  // Tasks
  getTasks(categoryId?: string): Observable<HabitTask[]> {
    const params = categoryId ? `?categoryId=${categoryId}` : '';
    return this.http.get<HabitTask[]>(`${this.apiUrl}/habittasks${params}`);
  }

  getTaskById(id: string): Observable<HabitTask> {
    return this.http.get<HabitTask>(`${this.apiUrl}/habittasks/${id}`);
  }

  getDailyTasks(date: string): Observable<DailyTaskView[]> {
    return this.http.get<DailyTaskView[]>(
      `${this.apiUrl}/habittasks/daily?date=${date}`);
  }

  createTask(request: CreateHabitTaskRequest): Observable<HabitTask> {
    return this.http.post<HabitTask>(`${this.apiUrl}/habittasks`, request);
  }

  updateTask(id: string, request: UpdateHabitTaskRequest): Observable<HabitTask> {
    return this.http.patch<HabitTask>(`${this.apiUrl}/habittasks/${id}`, request);
  }

  deleteTask(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/habittasks/${id}`);
  }

  // Completions
  logCompletion(request: LogCompletionRequest): Observable<any> {
    return this.http.post(`${this.apiUrl}/completions`, request);
  }

  undoCompletion(completionId: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/completions/${completionId}`);
  }
}