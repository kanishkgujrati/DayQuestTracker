import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environments';
import {
  HabitTask,
  DailyTaskView,
  CreateHabitTaskRequest,
  UpdateHabitTaskRequest,
  LogCompletionRequest,
} from '../models/task.model';

@Injectable({ providedIn: 'root' })
export class TaskService {
  private readonly tasksapiUrl = `${environment.apiUrl}/habittasks`;
  private readonly completionapiUrl = `${environment.apiUrl}/completions`;

  constructor(private http: HttpClient) {}

  // Tasks
  getTasks(categoryId?: string): Observable<HabitTask[]> {
    const params = categoryId ? `?categoryId=${categoryId}` : '';
    return this.http.get<HabitTask[]>(`${this.tasksapiUrl}${params}`);
  }

  getTaskById(id: string): Observable<HabitTask> {
    return this.http.get<HabitTask>(`${this.tasksapiUrl}/${id}`);
  }

  getDailyTasks(date: string): Observable<DailyTaskView[]> {
    return this.http.get<DailyTaskView[]>(`${this.tasksapiUrl}/dailyTasks?date=${date}`);
  }

  createTask(request: CreateHabitTaskRequest): Observable<HabitTask> {
    return this.http.post<HabitTask>(`${this.tasksapiUrl}`, request);
  }

  updateTask(id: string, request: UpdateHabitTaskRequest): Observable<HabitTask> {
    return this.http.patch<HabitTask>(`${this.tasksapiUrl}/${id}`, request);
  }

  deleteTask(id: string): Observable<void> {
    return this.http.delete<void>(`${this.tasksapiUrl}/${id}`);
  }

  // Completions
  logCompletion(request: LogCompletionRequest): Observable<any> {
    return this.http.post(`${this.completionapiUrl}`, request);
  }

  undoCompletion(completionId: string): Observable<void> {
    return this.http.delete<void>(`${this.completionapiUrl}/${completionId}`);
  }
}
