import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environments';
import {
  TaskConsistency,
  DailyScoreTrend,
  TaskStreakSummary,
  CategoryPerformance,
  WeakestHabit,
} from '../models/analytics.models';

@Injectable({ providedIn: 'root' })
export class AnalyticsService {
  private readonly apiUrl = `${environment.apiUrl}/Analytics`;

  constructor(private http: HttpClient) {}

  getConsistency(
    startDate: string,
    endDate: string,
    categoryId?: string,
  ): Observable<TaskConsistency[]> {
    const params: any = {
      startDate,
      endDate,
    };

    if (categoryId) {
      params.categoryId = categoryId;
    }

    return this.http.get<TaskConsistency[]>(`${this.apiUrl}/consistency`, { params });
  }

  getDailyTrend(startDate: string, endDate: string): Observable<DailyScoreTrend[]> {
    return this.http.get<DailyScoreTrend[]>(`${this.apiUrl}/daily-trend`, {
      params: {
        startDate,
        endDate,
      },
    });
  }

  getStreaks(): Observable<TaskStreakSummary[]> {
    return this.http.get<TaskStreakSummary[]>(`${this.apiUrl}/streaks`);
  }

  getWeakestHabits(startDate: string, endDate: string, topN = 5): Observable<WeakestHabit[]> {
    return this.http.get<WeakestHabit[]>(`${this.apiUrl}/weakest-habits`, {
      params: {
        startDate,
        endDate,
        topN,
      },
    });
  }

  getCategoryPerformance(startDate: string, endDate: string): Observable<CategoryPerformance[]> {
    return this.http.get<CategoryPerformance[]>(`${this.apiUrl}/category-performance`, {
      params: {
        startDate,
        endDate,
      },
    });
  }

  getWeeklySummary(date: string): Observable<any> {
    return this.http.get(`${this.apiUrl}/weekly-summary/${date}`);
  }

  getMonthlySummary(year: number, month: number): Observable<any> {
    return this.http.get(`${this.apiUrl}/monthly-summary/${year}/${month}`);
  }

  getDayHistory(date: string): Observable<any> {
    return this.http.get(`${this.apiUrl}/history/${date}`);
  }
}
