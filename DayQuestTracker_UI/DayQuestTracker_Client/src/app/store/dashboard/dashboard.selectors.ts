import { createFeatureSelector, createSelector } from '@ngrx/store';
import { DashboardState } from './dashboard.reducer';
import { CompletionStatus } from '../../core/models/task.models';

export const selectDashboardState = createFeatureSelector<DashboardState>('dashboard');

export const selectDailyTasks = createSelector(selectDashboardState, (state) => state.dailyTasks);

export const selectSelectedDate = createSelector(
  selectDashboardState,
  (state) => state.selectedDate,
);

export const selectDashboardLoading = createSelector(
  selectDashboardState,
  (state) => state.isLoading,
);

export const selectDashboardError = createSelector(selectDashboardState, (state) => state.error);

export const selectDailyProgress = createSelector(selectDailyTasks, (tasks) => {
  const total = tasks.length;
  const completed = tasks.filter((t) => t.status === CompletionStatus.Completed).length;
  const skipped = tasks.filter((t) => t.status === CompletionStatus.Skipped).length;
  const pending = tasks.filter((t) => t.status === null).length;
  const score = total > 0 ? Math.round((completed / total) * 100) : 0;

  return { total, completed, skipped, pending, score };
});
