import { Injectable, inject } from '@angular/core';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { catchError, forkJoin, map, switchMap } from 'rxjs';
import { of } from 'rxjs';
import { AnalyticsService } from '../../core/services/analytic.service';
import * as AnalyticsActions from './analytics.actions';

@Injectable()
export class AnalyticsEffects {
  private actions$ = inject(Actions);
  private analyticsService = inject(AnalyticsService);

  loadAnalytics$ = createEffect(() =>
    this.actions$.pipe(
      ofType(AnalyticsActions.loadAnalytics),
      switchMap(({ startDate, endDate }) =>
        forkJoin({
          consistency: this.analyticsService.getConsistency(startDate, endDate),
          dailyTrend: this.analyticsService.getDailyTrend(startDate, endDate),
          streaks: this.analyticsService.getStreaks(),
          weakestHabits: this.analyticsService.getWeakestHabits(startDate, endDate),
          categoryPerformance: this.analyticsService.getCategoryPerformance(startDate, endDate),
        }).pipe(
          map(({ consistency, dailyTrend, streaks, weakestHabits, categoryPerformance }) => {
            // Dispatch individual success actions
            return [
              AnalyticsActions.loadConsistencySuccess({ data: consistency }),
              AnalyticsActions.loadDailyTrendSuccess({ data: dailyTrend }),
              AnalyticsActions.loadStreaksSuccess({ data: streaks }),
              AnalyticsActions.loadWeakestHabitsSuccess({ data: weakestHabits }),
              AnalyticsActions.loadCategoryPerformanceSuccess({
                data: categoryPerformance,
              }),
            ];
          }),
          switchMap((actions) => of(...actions)),
          catchError((error) =>
            of(
              AnalyticsActions.loadAnalyticsFailure({
                error: error.error?.error || 'Failed to load analytics.',
              }),
            ),
          ),
        ),
      ),
    ),
  );
}
