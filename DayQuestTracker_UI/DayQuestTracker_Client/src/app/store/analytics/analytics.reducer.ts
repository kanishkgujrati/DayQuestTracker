import { createReducer, on } from '@ngrx/store';
import {
  TaskConsistency,
  DailyScoreTrend,
  TaskStreakSummary,
  CategoryPerformance,
  WeakestHabit,
} from '../../core/models/analytics.models';
import * as AnalyticsActions from './analytics.actions';

export interface AnalyticsState {
  consistency: TaskConsistency[];
  dailyTrend: DailyScoreTrend[];
  streaks: TaskStreakSummary[];
  weakestHabits: WeakestHabit[];
  categoryPerformance: CategoryPerformance[];
  startDate: string;
  endDate: string;
  isLoading: boolean;
  error: string | null;
}

const getDefaultDates = () => {
  const end = new Date();
  const start = new Date();
  start.setDate(end.getDate() - 29); // last 30 days
  return {
    start: start.toISOString().split('T')[0],
    end: end.toISOString().split('T')[0],
  };
};

const defaults = getDefaultDates();

export const initialState: AnalyticsState = {
  consistency: [],
  dailyTrend: [],
  streaks: [],
  weakestHabits: [],
  categoryPerformance: [],
  startDate: defaults.start,
  endDate: defaults.end,
  isLoading: false,
  error: null,
};

export const analyticsReducer = createReducer(
  initialState,

  on(AnalyticsActions.loadAnalytics, (state) => ({
    ...state,
    isLoading: true,
    error: null,
  })),

  on(AnalyticsActions.loadConsistencySuccess, (state, { data }) => ({
    ...state,
    consistency: data,
  })),

  on(AnalyticsActions.loadDailyTrendSuccess, (state, { data }) => ({
    ...state,
    dailyTrend: data,
    isLoading: false,
  })),

  on(AnalyticsActions.loadStreaksSuccess, (state, { data }) => ({
    ...state,
    streaks: data,
  })),

  on(AnalyticsActions.loadWeakestHabitsSuccess, (state, { data }) => ({
    ...state,
    weakestHabits: data,
  })),

  on(AnalyticsActions.loadCategoryPerformanceSuccess, (state, { data }) => ({
    ...state,
    categoryPerformance: data,
  })),

  on(AnalyticsActions.loadAnalyticsFailure, (state, { error }) => ({
    ...state,
    isLoading: false,
    error,
  })),

  on(AnalyticsActions.setAnalyticsDateRange, (state, { startDate, endDate }) => ({
    ...state,
    startDate,
    endDate,
  })),
);
