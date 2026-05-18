import { createAction, props } from '@ngrx/store';
import {
  TaskConsistency,
  DailyScoreTrend,
  TaskStreakSummary,
  CategoryPerformance,
  WeakestHabit,
} from '../../core/models/analytics.models';

export const loadAnalytics = createAction(
  '[Analytics] Load All',
  props<{ startDate: string; endDate: string }>(),
);

export const loadConsistencySuccess = createAction(
  '[Analytics] Load Consistency Success',
  props<{ data: TaskConsistency[] }>(),
);

export const loadDailyTrendSuccess = createAction(
  '[Analytics] Load Daily Trend Success',
  props<{ data: DailyScoreTrend[] }>(),
);

export const loadStreaksSuccess = createAction(
  '[Analytics] Load Streaks Success',
  props<{ data: TaskStreakSummary[] }>(),
);

export const loadWeakestHabitsSuccess = createAction(
  '[Analytics] Load Weakest Habits Success',
  props<{ data: WeakestHabit[] }>(),
);

export const loadCategoryPerformanceSuccess = createAction(
  '[Analytics] Load Category Performance Success',
  props<{ data: CategoryPerformance[] }>(),
);

export const loadAnalyticsFailure = createAction(
  '[Analytics] Load Failure',
  props<{ error: string }>(),
);

export const setAnalyticsDateRange = createAction(
  '[Analytics] Set Date Range',
  props<{ startDate: string; endDate: string }>(),
);
