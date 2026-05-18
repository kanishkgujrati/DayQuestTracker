import { createFeatureSelector, createSelector } from '@ngrx/store';
import { AnalyticsState } from './analytics.reducer';

export const selectAnalyticsState = createFeatureSelector<AnalyticsState>('analytics');

export const selectConsistency = createSelector(selectAnalyticsState, (state) => state.consistency);

export const selectDailyTrend = createSelector(selectAnalyticsState, (state) => state.dailyTrend);

export const selectStreaks = createSelector(selectAnalyticsState, (state) => state.streaks);

export const selectWeakestHabits = createSelector(
  selectAnalyticsState,
  (state) => state.weakestHabits,
);

export const selectCategoryPerformance = createSelector(
  selectAnalyticsState,
  (state) => state.categoryPerformance,
);

export const selectAnalyticsLoading = createSelector(
  selectAnalyticsState,
  (state) => state.isLoading,
);

export const selectAnalyticsDateRange = createSelector(selectAnalyticsState, (state) => ({
  startDate: state.startDate,
  endDate: state.endDate,
}));

export const selectTopStreaks = createSelector(selectStreaks, (streaks) =>
  [...streaks].sort((a, b) => b.currentStreak - a.currentStreak).slice(0, 5),
);

export const selectAverageConsistency = createSelector(selectConsistency, (consistency) => {
  if (!consistency.length) return 0;
  const avg = consistency.reduce((sum, c) => sum + c.consistencyPercent, 0) / consistency.length;
  return Math.round(avg * 10) / 10;
});

export const selectTotalXPInPeriod = createSelector(selectDailyTrend, (trend) =>
  trend.reduce((sum, d) => sum + d.xpEarned, 0),
);

export const selectPerfectDays = createSelector(
  selectDailyTrend,
  (trend) => trend.filter((d) => d.score === 100).length,
);
